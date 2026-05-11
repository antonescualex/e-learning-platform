package licenta.facultate.alex.backendlicenta.config;

import jakarta.validation.Valid;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.validation.annotation.Validated;

import java.time.Duration;
import java.util.List;

@Validated
@ConfigurationProperties(prefix = "app")
public record AppProperties(
        @Valid OpenAi openai,
        @Valid Cors cors,
        @Valid Auth auth
) {
    public record OpenAi(
            @NotBlank String baseUrl,
            @NotBlank String apiKey,
            @NotBlank String model,
            @Min(200) @Max(10000) int maxOutputTokens,
            @Min(0) @Max(2) int maxRetries,
            @NotNull Duration timeout
    ) {
    }

    public record Cors(
            @NotNull List<@NotBlank String> allowedOrigins
    ) {
    }

    public record Auth(
            @Valid Jwt jwt
    ) {
    }

    public record Jwt(
            @NotBlank String issuer,
            @NotBlank String secret,
            @NotNull Duration accessTokenTtl,
            @NotNull Duration refreshTokenTtl
    ) {
    }
}
