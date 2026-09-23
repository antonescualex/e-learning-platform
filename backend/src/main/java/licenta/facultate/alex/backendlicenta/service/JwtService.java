package licenta.facultate.alex.backendlicenta.service;

import licenta.facultate.alex.backendlicenta.config.AppProperties;
import licenta.facultate.alex.backendlicenta.entity.UserCredentials;
import licenta.facultate.alex.backendlicenta.exception.ApiException;
import org.springframework.http.HttpStatus;
import org.springframework.security.oauth2.jose.jws.MacAlgorithm;
import org.springframework.security.oauth2.jwt.*;
import org.springframework.stereotype.Service;

import javax.crypto.SecretKey;
import java.time.Duration;
import java.time.Instant;

@Service
public class JwtService {
    private static final MacAlgorithm JWT_ALGORITHM = MacAlgorithm.HS256;

    private final JwtEncoder jwtEncoder;
    private final JwtDecoder refreshTokenDecoder;
    private final AppProperties properties;

    public JwtService(JwtEncoder jwtEncoder, SecretKey jwtSecretKey, AppProperties properties) {
        this.jwtEncoder = jwtEncoder;
        this.properties = properties;

        NimbusJwtDecoder decoder = NimbusJwtDecoder.withSecretKey(jwtSecretKey)
                .macAlgorithm(JWT_ALGORITHM)
                .build();
        decoder.setJwtValidator(JwtValidators.createDefaultWithIssuer(properties.auth().jwt().issuer()));
        this.refreshTokenDecoder = decoder;
    }

    public IssuedTokens issueTokens(UserCredentials credentials) {
        Instant now = Instant.now();
        Duration accessTtl = properties.auth().jwt().accessTokenTtl();
        Duration refreshTtl = properties.auth().jwt().refreshTokenTtl();

        return new IssuedTokens(
                createToken(credentials, now, now.plus(accessTtl), "access"),
                createToken(credentials, now, now.plus(refreshTtl), "refresh"),
                accessTtl.toSeconds(),
                refreshTtl.toSeconds()
        );
    }

    public RefreshTokenClaims validateRefreshToken(String refreshToken) {
        try {
            Jwt jwt = refreshTokenDecoder.decode(refreshToken);

            if (!"refresh".equals(jwt.getClaimAsString("type"))) {
                throw new ApiException(HttpStatus.UNAUTHORIZED, "INVALID_REFRESH_TOKEN", "Refresh token is invalid");
            }

            Number tokenVersionNumber = jwt.getClaim("token_version");
            if (tokenVersionNumber == null) {
                throw new ApiException(HttpStatus.UNAUTHORIZED, "INVALID_REFRESH_TOKEN", "Refresh token is invalid");
            }

            return new RefreshTokenClaims(
                    jwt.getSubject(),
                    jwt.getClaimAsString("username"),
                    tokenVersionNumber.intValue()
            );
        } catch (JwtException ex) {
            throw new ApiException(HttpStatus.UNAUTHORIZED, "INVALID_REFRESH_TOKEN", "Refresh token is invalid");
        }
    }

    private String createToken(UserCredentials credentials, Instant issuedAt, Instant expiresAt, String tokenType) {
        Integer tokenVersion = credentials.getTokenVersion() == null ? 0 : credentials.getTokenVersion();

        JwtClaimsSet claims = JwtClaimsSet.builder()
                .issuer(properties.auth().jwt().issuer())
                .subject(credentials.getUserId())
                .issuedAt(issuedAt)
                .notBefore(issuedAt)
                .expiresAt(expiresAt)
                .claim("username", credentials.getUsername())
                .claim("token_version", tokenVersion)
                .claim("type", tokenType)
                .build();

        JwsHeader jwsHeader = JwsHeader.with(JWT_ALGORITHM)
                .type("JWT")
                .build();

        return jwtEncoder.encode(JwtEncoderParameters.from(jwsHeader, claims)).getTokenValue();
    }

    public record IssuedTokens(
            String accessToken,
            String refreshToken,
            long accessTokenExpiresInSeconds,
            long refreshTokenExpiresInSeconds
    ) {
    }

    public record RefreshTokenClaims(
            String userId,
            String username,
            int tokenVersion
    ) {
    }
}
