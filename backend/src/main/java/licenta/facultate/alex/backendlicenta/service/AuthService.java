package licenta.facultate.alex.backendlicenta.service;

import jakarta.persistence.EntityManager;
import licenta.facultate.alex.backendlicenta.dto.api.AuthDtos;
import licenta.facultate.alex.backendlicenta.entity.UserAvatar;
import licenta.facultate.alex.backendlicenta.entity.UserBackground;
import licenta.facultate.alex.backendlicenta.entity.UserCredentials;
import licenta.facultate.alex.backendlicenta.entity.UserProfile;
import licenta.facultate.alex.backendlicenta.exception.ApiException;
import licenta.facultate.alex.backendlicenta.repository.UserAvatarRepository;
import licenta.facultate.alex.backendlicenta.repository.UserBackgroundRepository;
import licenta.facultate.alex.backendlicenta.repository.UserCredentialsRepository;
import licenta.facultate.alex.backendlicenta.repository.UserProfileRepository;
import org.springframework.http.HttpStatus;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.UUID;

@Service
public class AuthService {
    private static final List<String> DEFAULT_AVATAR_IDS = List.of(
            "boy_3",
            "girl_1",
            "boy_1",
            "girl_3",
            "boy_4",
            "girl_2",
            "boy_2",
            "girl_4"
    );

    private final UserCredentialsRepository userCredentialsRepository;
    private final UserProfileRepository userProfileRepository;
    private final UserAvatarRepository userAvatarRepository;
    private final UserBackgroundRepository userBackgroundRepository;
    private final PasswordEncoder passwordEncoder;
    private final EntityManager entityManager;
    private final JwtService jwtService;

    public AuthService(
            UserCredentialsRepository userCredentialsRepository,
            UserProfileRepository userProfileRepository,
            UserAvatarRepository userAvatarRepository,
            UserBackgroundRepository userBackgroundRepository,
            PasswordEncoder passwordEncoder,
            EntityManager entityManager,
            JwtService jwtService
    ) {
        this.userCredentialsRepository = userCredentialsRepository;
        this.userProfileRepository = userProfileRepository;
        this.userAvatarRepository = userAvatarRepository;
        this.userBackgroundRepository = userBackgroundRepository;
        this.passwordEncoder = passwordEncoder;
        this.entityManager = entityManager;
        this.jwtService = jwtService;
    }

    @Transactional
    public AuthDtos.RegisterResponse register(AuthDtos.RegisterRequest request) {
        String username = request.username().trim();
        String playerName = request.playerName().trim();

        if (userCredentialsRepository.existsByUsername(username)) {
            throw new ApiException(HttpStatus.CONFLICT, "USERNAME_TAKEN", "Username already exists");
        }

        String userId = UUID.randomUUID().toString();
        String passwordHash = passwordEncoder.encode(request.password());

        UserCredentials credentials = new UserCredentials(userId, username, passwordHash);
        entityManager.persist(credentials);

        UserProfile profile = new UserProfile(credentials, playerName);
        credentials.attachProfile(profile);
        entityManager.persist(profile);

        profile.setSelectedAvatarId("boy_3");
        profile.setSelectedBackgroundId("default_background");

        for (String avatarId : DEFAULT_AVATAR_IDS) {
            UserAvatar avatar = new UserAvatar(profile, avatarId);
            profile.addAvatar(avatar);
            entityManager.persist(avatar);
        }

        UserBackground defaultBackground = new UserBackground(profile, "default_background");
        profile.addBackground(defaultBackground);
        entityManager.persist(defaultBackground);

        entityManager.flush();

        return new AuthDtos.RegisterResponse(
                credentials.getUserId(),
                credentials.getUsername(),
                profile.getPlayerName()
        );
    }

    @Transactional(readOnly = true)
    public AuthDtos.AuthResponse login(AuthDtos.LoginRequest request) {
        String username = request.username().trim();

        UserCredentials credentials = userCredentialsRepository.findByUsername(username)
                .orElseThrow(this::invalidCredentials);

        if (!Boolean.TRUE.equals(credentials.getActive())) {
            throw new ApiException(HttpStatus.FORBIDDEN, "ACCOUNT_DISABLED", "Account is disabled");
        }

        if (!passwordEncoder.matches(request.password(), credentials.getPasswordHash())) {
            throw invalidCredentials();
        }

        UserProfile profile = loadRequiredProfile(credentials.getUserId());
        JwtService.IssuedTokens tokens = jwtService.issueTokens(credentials);

        return toAuthResponse(credentials, profile, tokens);
    }

    @Transactional(readOnly = true)
    public AuthDtos.AuthResponse refresh(AuthDtos.RefreshRequest request) {
        JwtService.RefreshTokenClaims claims = jwtService.validateRefreshToken(request.refreshToken());

        UserCredentials credentials = userCredentialsRepository.findById(claims.userId())
                .orElseThrow(() -> new ApiException(HttpStatus.UNAUTHORIZED, "INVALID_REFRESH_TOKEN", "Refresh token is invalid"));

        if (!Boolean.TRUE.equals(credentials.getActive())) {
            throw new ApiException(HttpStatus.FORBIDDEN, "ACCOUNT_DISABLED", "Account is disabled");
        }

        Integer currentTokenVersion = credentials.getTokenVersion() == null ? 0 : credentials.getTokenVersion();
        if (!credentials.getUsername().equals(claims.username()) || currentTokenVersion != claims.tokenVersion()) {
            throw new ApiException(HttpStatus.UNAUTHORIZED, "INVALID_REFRESH_TOKEN", "Refresh token is invalid");
        }

        UserProfile profile = loadRequiredProfile(credentials.getUserId());
        JwtService.IssuedTokens tokens = jwtService.issueTokens(credentials);

        return toAuthResponse(credentials, profile, tokens);
    }

    @Transactional
    public AuthDtos.LogoutAllResponse logoutAll(String userId) {
        UserCredentials credentials = userCredentialsRepository.findById(userId)
                .orElseThrow(() -> new ApiException(
                        HttpStatus.UNAUTHORIZED,
                        "INVALID_ACCESS_TOKEN",
                        "Authenticated user was not found"
                ));

        int currentTokenVersion = credentials.getTokenVersion() == null ? 0 : credentials.getTokenVersion();
        credentials.setTokenVersion(currentTokenVersion + 1);

        return new AuthDtos.LogoutAllResponse("All sessions have been invalidated");
    }

    private AuthDtos.AuthResponse toAuthResponse(
            UserCredentials credentials,
            UserProfile profile,
            JwtService.IssuedTokens tokens
    ) {
        return new AuthDtos.AuthResponse(
                tokens.accessToken(),
                tokens.refreshToken(),
                "Bearer",
                tokens.accessTokenExpiresInSeconds(),
                tokens.refreshTokenExpiresInSeconds(),
                new AuthDtos.AuthenticatedUser(
                        credentials.getUserId(),
                        credentials.getUsername(),
                        profile.getPlayerName()
                )
        );
    }

    private UserProfile loadRequiredProfile(String userId) {
        return userProfileRepository.findById(userId)
                .orElseThrow(() -> new ApiException(
                        HttpStatus.INTERNAL_SERVER_ERROR,
                        "PROFILE_MISSING",
                        "User profile is missing"
                ));
    }

    private ApiException invalidCredentials() {
        return new ApiException(HttpStatus.UNAUTHORIZED, "INVALID_CREDENTIALS", "Invalid username or password");
    }
}
