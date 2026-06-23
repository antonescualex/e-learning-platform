package licenta.facultate.alex.backendlicenta.service;

import licenta.facultate.alex.backendlicenta.service.BadgeAwardService;
import licenta.facultate.alex.backendlicenta.dto.api.MeDtos;
import licenta.facultate.alex.backendlicenta.entity.UserBooster;
import licenta.facultate.alex.backendlicenta.entity.UserProfile;
import licenta.facultate.alex.backendlicenta.entity.id.UserBoosterId;
import licenta.facultate.alex.backendlicenta.exception.ApiException;
import licenta.facultate.alex.backendlicenta.repository.UserAvatarRepository;
import licenta.facultate.alex.backendlicenta.repository.UserBackgroundRepository;
import licenta.facultate.alex.backendlicenta.repository.UserBadgeRepository;
import licenta.facultate.alex.backendlicenta.repository.UserBoosterRepository;
import licenta.facultate.alex.backendlicenta.repository.UserProfileRepository;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Duration;
import java.time.LocalDate;
import java.time.OffsetDateTime;
import java.util.List;
import java.util.Map;

@Service
public class MeService {

    private static final int MIN_LENGTH = 3;
    private static final int MAX_LENGTH = 100;

    private static final Map<String, BoosterActivation> BOOSTER_ACTIVATIONS = Map.of(
            "booster_double_xp_10m", new BoosterActivation(BoosterEffect.DOUBLE_XP, Duration.ofMinutes(10)),
            "booster_double_xp_30m", new BoosterActivation(BoosterEffect.DOUBLE_XP, Duration.ofMinutes(30)),
            "booster_double_xp_1h", new BoosterActivation(BoosterEffect.DOUBLE_XP, Duration.ofHours(1)),
            "booster_double_coins_10m", new BoosterActivation(BoosterEffect.DOUBLE_COINS, Duration.ofMinutes(10)),
            "booster_double_coins_30m", new BoosterActivation(BoosterEffect.DOUBLE_COINS, Duration.ofMinutes(30)),
            "booster_double_coins_1h", new BoosterActivation(BoosterEffect.DOUBLE_COINS, Duration.ofHours(1))
    );

    private final UserProfileRepository userProfileRepository;
    private final UserBadgeRepository userBadgeRepository;
    private final UserAvatarRepository userAvatarRepository;
    private final UserBackgroundRepository userBackgroundRepository;
    private final UserBoosterRepository userBoosterRepository;
    private final BadgeAwardService badgeAwardService;
    private final LevelService levelService;

    public MeService(
            UserProfileRepository userProfileRepository,
            UserBadgeRepository userBadgeRepository,
            UserAvatarRepository userAvatarRepository,
            UserBackgroundRepository userBackgroundRepository,
            UserBoosterRepository userBoosterRepository,
            BadgeAwardService badgeAwardService,
            LevelService levelService
    ) {
        this.userProfileRepository = userProfileRepository;
        this.userBadgeRepository = userBadgeRepository;
        this.userAvatarRepository = userAvatarRepository;
        this.userBackgroundRepository = userBackgroundRepository;
        this.userBoosterRepository = userBoosterRepository;
        this.badgeAwardService = badgeAwardService;
        this.levelService = levelService;
    }

    @Transactional(readOnly = true)
    public MeDtos.ProfileResponse getProfile(String userId, String username) {
        UserProfile profile = loadRequiredProfile(userId);

        List<String> ownedBadgeIds = userBadgeRepository.findAllById_UserIdOrderByUnlockedAtAsc(userId).stream()
                .map(badge -> badge.getId().getBadgeId())
                .toList();

        List<String> ownedAvatarIds = userAvatarRepository.findAllById_UserIdOrderByAcquiredAtAsc(userId).stream()
                .map(avatar -> avatar.getId().getAvatarId())
                .toList();

        List<String> ownedBackgroundIds = userBackgroundRepository.findAllById_UserIdOrderByAcquiredAtAsc(userId).stream()
                .map(background -> background.getId().getBackgroundId())
                .toList();

        List<MeDtos.BoosterDto> boosters = userBoosterRepository.findAllById_UserIdOrderByUpdatedAtAsc(userId).stream()
                .map(booster -> new MeDtos.BoosterDto(
                        booster.getId().getBoosterItemId(),
                        booster.getQuantity()
                ))
                .toList();

        return new MeDtos.ProfileResponse(
                userId,
                username,
                profile.getPlayerName(),
                profile.getLevelNumber(),
                profile.getCurrentExperience(),
                levelService.experienceNeededForNextLevel(profile.getLevelNumber()),
                profile.getCoins(),
                profile.getSelectedAvatarId(),
                profile.getSelectedBackgroundId(),
                profile.getCreatedAt(),
                profile.getLastLoginDate(),
                profile.getCurrentLoginStreak(),
                profile.getCompletedLessonsCount(),
                profile.getIncompleteLessonsCount(),
                profile.getTotalShopPurchases(),
                profile.getTotalCoinsSpentInShop(),
                profile.getDoubleCoinsExpiresAt(),
                profile.getDoubleXpExpiresAt(),
                ownedBadgeIds,
                ownedAvatarIds,
                ownedBackgroundIds,
                boosters
        );
    }

    @Transactional
    public MeDtos.ProfileResponse updateProfile(String userId, String username, MeDtos.UpdateProfileRequest request) {
        UserProfile profile = loadRequiredProfile(userId);
        String playerName = request.playerName().trim();

        if (playerName.length() < MIN_LENGTH || playerName.length() > MAX_LENGTH) {
            throw new ApiException(
                    HttpStatus.BAD_REQUEST,
                    "INVALID_PLAYER_NAME",
                    "Player name must be between " + MIN_LENGTH + " and " + MAX_LENGTH + " characters."
            );
        }

        profile.setPlayerName(playerName);
        return getProfile(userId, username);
    }

    @Transactional
    public MeDtos.ProfileResponse selectAvatar(String userId, String username, MeDtos.SelectAvatarRequest request) {
        UserProfile profile = loadRequiredProfile(userId);

        String avatarId = request.avatarId().trim();

        boolean owned = userAvatarRepository.existsById_UserIdAndId_AvatarId(userId, avatarId);
        if (!owned) {
            throw new ApiException(
                    HttpStatus.BAD_REQUEST,
                    "AVATAR_NOT_OWNED",
                    "Avatar is not owned by the current user"
            );
        }

        profile.setSelectedAvatarId(avatarId);

        return getProfile(userId, username);
    }

    @Transactional
    public MeDtos.ProfileResponse selectBackground(String userId, String username, MeDtos.SelectBackgroundRequest request) {
        UserProfile profile = loadRequiredProfile(userId);

        String backgroundId = request.backgroundId().trim();

        boolean owned = userBackgroundRepository.existsById_UserIdAndId_BackgroundId(userId, backgroundId);
        if (!owned) {
            throw new ApiException(
                    HttpStatus.BAD_REQUEST,
                    "BACKGROUND_NOT_OWNED",
                    "Background is not owned by the current user"
            );
        }

        profile.setSelectedBackgroundId(backgroundId);

        return getProfile(userId, username);
    }

    private UserProfile loadRequiredProfile(String userId) {
        return userProfileRepository.findById(userId)
                .orElseThrow(() -> new ApiException(
                        HttpStatus.INTERNAL_SERVER_ERROR,
                        "PROFILE_MISSING",
                        "User profile is missing"
                ));
    }

    @Transactional
    public MeDtos.ProfileUpdateResponse dailyLogin(String userId, String username) {
        UserProfile profile = loadRequiredProfile(userId);
        LocalDate today = LocalDate.now();
        LocalDate lastLoginDate = profile.getLastLoginDate();

        if (lastLoginDate == null) {
            profile.setLastLoginDate(today);
            profile.setCurrentLoginStreak(1);
        } else if (lastLoginDate.isEqual(today)) {
            // Already claimed today. Do not increment streak again.
        } else if (lastLoginDate.plusDays(1).isEqual(today)) {
            profile.setLastLoginDate(today);
            profile.setCurrentLoginStreak(defaultZero(profile.getCurrentLoginStreak()) + 1);
        } else {
            profile.setLastLoginDate(today);
            profile.setCurrentLoginStreak(1);
        }

        List<String> awardedBadgeIds = badgeAwardService.awardDailyBadges(profile);

        return new MeDtos.ProfileUpdateResponse(
                getProfile(userId, username),
                awardedBadgeIds
        );
    }

    @Transactional
    public MeDtos.ProfileResponse activateBooster(
            String userId,
            String username,
            MeDtos.ActivateBoosterRequest request
    ) {
        String boosterItemId = request.boosterItemId().trim();

        BoosterActivation activation = BOOSTER_ACTIVATIONS.get(boosterItemId);
        if (activation == null) {
            throw new ApiException(
                    HttpStatus.BAD_REQUEST,
                    "UNSUPPORTED_BOOSTER",
                    "Booster is not supported"
            );
        }

        UserProfile profile = loadRequiredProfile(userId);
        OffsetDateTime now = OffsetDateTime.now();

        normalizeExpiredBoosters(profile, now);

        if (isBoosterActive(profile.getDoubleXpExpiresAt(), now)
                || isBoosterActive(profile.getDoubleCoinsExpiresAt(), now)) {
            throw new ApiException(
                    HttpStatus.CONFLICT,
                    "BOOSTER_ALREADY_ACTIVE",
                    "A booster is already active"
            );
        }

        UserBoosterId boosterKey = new UserBoosterId(userId, boosterItemId);
        UserBooster booster = userBoosterRepository.findById(boosterKey)
                .orElseThrow(() -> new ApiException(
                        HttpStatus.BAD_REQUEST,
                        "BOOSTER_NOT_OWNED",
                        "Booster is not owned by the current user"
                ));

        int currentQuantity = defaultZero(booster.getQuantity());
        if (currentQuantity < 1) {
            throw new ApiException(
                    HttpStatus.BAD_REQUEST,
                    "BOOSTER_NOT_AVAILABLE",
                    "Booster quantity is not available"
            );
        }

        int remainingQuantity = currentQuantity - 1;
        if (remainingQuantity == 0) {
            userBoosterRepository.delete(booster);
        } else {
            booster.setQuantity(remainingQuantity);
        }

        OffsetDateTime expiresAt = now.plus(activation.duration());

        if (activation.effect() == BoosterEffect.DOUBLE_XP) {
            profile.setDoubleXpExpiresAt(expiresAt);
        } else if (activation.effect() == BoosterEffect.DOUBLE_COINS) {
            profile.setDoubleCoinsExpiresAt(expiresAt);
        }

        return getProfile(userId, username);
    }

    private int defaultZero(Integer value) {
        return value == null ? 0 : value;
    }

    private void normalizeExpiredBoosters(UserProfile profile, OffsetDateTime now) {
        if (profile.getDoubleXpExpiresAt() != null && !profile.getDoubleXpExpiresAt().isAfter(now)) {
            profile.setDoubleXpExpiresAt(null);
        }

        if (profile.getDoubleCoinsExpiresAt() != null && !profile.getDoubleCoinsExpiresAt().isAfter(now)) {
            profile.setDoubleCoinsExpiresAt(null);
        }
    }

    private boolean isBoosterActive(OffsetDateTime expiresAt, OffsetDateTime now) {
        return expiresAt != null && expiresAt.isAfter(now);
    }

    private enum BoosterEffect {
        DOUBLE_XP,
        DOUBLE_COINS
    }

    private record BoosterActivation(
            BoosterEffect effect,
            Duration duration
    ) {
    }
}