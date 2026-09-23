package licenta.facultate.alex.backendlicenta.service;

import tools.jackson.databind.ObjectMapper;
import licenta.facultate.alex.backendlicenta.config.AppProperties;
import licenta.facultate.alex.backendlicenta.dto.openai.OpenAiDtos;
import licenta.facultate.alex.backendlicenta.exception.ApiException;
import licenta.facultate.alex.backendlicenta.model.LessonRequestContext;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Component;
import org.springframework.web.client.RestClient;
import org.springframework.web.client.RestClientException;
import org.springframework.web.client.RestClientResponseException;

import java.util.List;
import java.util.Locale;
import java.util.function.Function;

@Component
public class OpenAiResponsesClient {

    private final RestClient restClient;
    private final ObjectMapper objectMapper;
    private final AppProperties properties;
    private final LessonDefinitionFactory definitionFactory;

    public OpenAiResponsesClient(RestClient openAiRestClient,
                                 ObjectMapper objectMapper,
                                 AppProperties properties,
                                 LessonDefinitionFactory definitionFactory) {
        this.restClient = openAiRestClient;
        this.objectMapper = objectMapper;
        this.properties = properties;
        this.definitionFactory = definitionFactory;
    }

    public <T> T generate(LessonRequestContext context,
                          Class<T> responseType,
                          Function<T, List<String>> semanticValidator) {
        String repairNotes = null;

        for (int attempt = 0; attempt <= properties.openai().maxRetries(); attempt++) {
            OpenAiDtos.ResponsesRequest request = new OpenAiDtos.ResponsesRequest(
                    properties.openai().model(),
                    false,
                    properties.openai().maxOutputTokens(),
                    definitionFactory.buildInstructions(context),
                    definitionFactory.buildInput(context, repairNotes),
                    new OpenAiDtos.Reasoning("high"),
                    new OpenAiDtos.Text(new OpenAiDtos.JsonSchemaFormat(
                            "json_schema",
                            context.lessonType().name().toLowerCase(Locale.ROOT) + "_lesson",
                            true,
                            definitionFactory.buildSchema(context)
                    ))
            );

            OpenAiDtos.ResponsesResponse response = callOpenAi(request);
            String outputText = extractOutputText(response);

            try {
                T parsed = objectMapper.readValue(outputText, responseType);
                List<String> semanticErrors = semanticValidator.apply(parsed);
                if (semanticErrors.isEmpty()) {
                    return parsed;
                }
                repairNotes = String.join(" | ", semanticErrors);
            } catch (Exception ex) {
                repairNotes = "Returned JSON could not be parsed into " + responseType.getSimpleName() + ": " + ex.getMessage();
            }
        }

        throw new ApiException(
                HttpStatus.BAD_GATEWAY,
                "AI_INVALID_RESPONSE",
                "OpenAI returned invalid content after retries"
        );
    }

    private OpenAiDtos.ResponsesResponse callOpenAi(OpenAiDtos.ResponsesRequest request) {
        try {
            OpenAiDtos.ResponsesResponse response = restClient.post()
                    .uri("/responses")
                    .body(request)
                    .retrieve()
                    .body(OpenAiDtos.ResponsesResponse.class);

            if (response == null) {
                throw new ApiException(HttpStatus.BAD_GATEWAY, "OPENAI_EMPTY_RESPONSE", "OpenAI returned an empty response");
            }
            if (response.error() != null) {
                throw new ApiException(
                        HttpStatus.BAD_GATEWAY,
                        "OPENAI_ERROR",
                        "OpenAI error: " + response.error().message()
                );
            }
            return response;
        } catch (RestClientResponseException ex) {
            throw new ApiException(
                    HttpStatus.BAD_GATEWAY,
                    "OPENAI_HTTP_ERROR",
                    "OpenAI returned HTTP " + ex.getStatusCode().value(),
                    List.of(ex.getResponseBodyAsString())
            );
        } catch (RestClientException ex) {
            throw new ApiException(
                    HttpStatus.SERVICE_UNAVAILABLE,
                    "OPENAI_UNAVAILABLE",
                    "Could not reach OpenAI"
            );
        }
    }

    private String extractOutputText(OpenAiDtos.ResponsesResponse response) {
        if (response.outputText() != null && !response.outputText().isBlank()) {
            return response.outputText();
        }

        StringBuilder sb = new StringBuilder();
        if (response.output() != null) {
            for (OpenAiDtos.OutputItem item : response.output()) {
                if (!"message".equals(item.type()) || item.content() == null) {
                    continue;
                }
                for (OpenAiDtos.ContentPart part : item.content()) {
                    if ("output_text".equals(part.type()) && part.text() != null) {
                        sb.append(part.text());
                    }
                }
            }
        }

        if (sb.isEmpty()) {
            throw new ApiException(HttpStatus.BAD_GATEWAY, "OPENAI_NO_TEXT", "No output_text found in OpenAI response");
        }

        return sb.toString();
    }
}
