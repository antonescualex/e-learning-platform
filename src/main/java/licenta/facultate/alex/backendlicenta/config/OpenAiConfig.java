package licenta.facultate.alex.backendlicenta.config;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.http.client.ClientHttpRequestFactory;
import org.springframework.http.client.SimpleClientHttpRequestFactory;
import org.springframework.web.client.RestClient;


@Configuration
public class OpenAiConfig {

    @Bean
    public RestClient.Builder restClientBuilder() {
        return RestClient.builder();
    }

    @Bean
    public RestClient openAiRestClient(RestClient.Builder builder, AppProperties properties) {
        return builder
                .baseUrl(properties.openai().baseUrl())
                .defaultHeader(HttpHeaders.AUTHORIZATION, "Bearer " + properties.openai().apiKey())
                .defaultHeader(HttpHeaders.CONTENT_TYPE, MediaType.APPLICATION_JSON_VALUE)
                .requestFactory(requestFactory(properties))
                .build();
    }

    private ClientHttpRequestFactory requestFactory(AppProperties properties) {
        SimpleClientHttpRequestFactory factory = new SimpleClientHttpRequestFactory();
        int timeoutMillis = (int) properties.openai().timeout().toMillis();
        factory.setConnectTimeout(timeoutMillis);
        factory.setReadTimeout(timeoutMillis);
        return factory;
    }
}
