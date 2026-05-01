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

import java.util.UUID;

@Service
public class AuthService {
    private final UserCredentialsRepository userCredentialsRepository;
    private final UserProfileRepository userProfileRepository;
    private final UserAvatarRepository userAvatarRepository;
    private final UserBackgroundRepository userBackgroundRepository;
    private final PasswordEncoder passwordEncoder;
    private final EntityManager entityManager;

    public AuthService(
            UserCredentialsRepository userCredentialsRepository,
            UserProfileRepository userProfileRepository,
            UserAvatarRepository userAvatarRepository,
            UserBackgroundRepository userBackgroundRepository,
            PasswordEncoder passwordEncoder,
            EntityManager entityManager
    ) {
        this.userCredentialsRepository = userCredentialsRepository;
        this.userProfileRepository = userProfileRepository;
        this.userAvatarRepository = userAvatarRepository;
        this.userBackgroundRepository = userBackgroundRepository;
        this.passwordEncoder = passwordEncoder;
        this.entityManager = entityManager;
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

        UserAvatar defaultAvatar = new UserAvatar(profile, "boy_3");
        UserBackground defaultBackground = new UserBackground(profile, "default_background");

        profile.addAvatar(defaultAvatar);
        profile.addBackground(defaultBackground);

        entityManager.persist(defaultAvatar);
        entityManager.persist(defaultBackground);

        entityManager.flush();

        return new AuthDtos.RegisterResponse(
                credentials.getUserId(),
                credentials.getUsername(),
                profile.getPlayerName()
        );
    }
}
