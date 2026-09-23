package licenta.facultate.alex.backendlicenta.dto.api;

import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Pattern;
import jakarta.validation.constraints.Size;

public final class AuthDtos {

    private AuthDtos() {
    }

    public record RegisterRequest(
            @JsonProperty("Username")
            @NotBlank
            @Size(min = 3, max = 50)
            @Pattern(regexp = "^[A-Za-z0-9._-]+$")
            String username,

            @JsonProperty("Password")
            @NotBlank
            @Size(min = 6, max = 72)
            String password,

            @JsonProperty("PlayerName")
            @NotBlank
            @Size(min = 3, max = 100)
            String playerName
    ){}

    public record RegisterResponse(
            @JsonProperty("UserId") String userId,
            @JsonProperty("Username") String username,
            @JsonProperty("PlayerName") String playerName
    ){}

    public record LoginRequest(
            @JsonProperty("Username")
            @NotBlank
            String username,

            @JsonProperty("Password")
            @NotBlank
            String password
    ) {}

    public record RefreshRequest(
            @JsonProperty("RefreshToken")
            @NotBlank
            String refreshToken
    ) {}

    public record AuthenticatedUser(
            @JsonProperty("UserId") String userId,
            @JsonProperty("Username") String username,
            @JsonProperty("PlayerName") String playerName
    ) {}

    public record AuthResponse(
            @JsonProperty("AccessToken") String accessToken,
            @JsonProperty("RefreshToken") String refreshToken,
            @JsonProperty("TokenType") String tokenType,
            @JsonProperty("AccessTokenExpiresInSeconds") long accessTokenExpiresInSeconds,
            @JsonProperty("RefreshTokenExpiresInSeconds") long refreshTokenExpiresInSeconds,
            @JsonProperty("User") AuthenticatedUser user
    ) {}

    public record LogoutAllResponse(
            @JsonProperty("Message") String message
    ) {}
}
