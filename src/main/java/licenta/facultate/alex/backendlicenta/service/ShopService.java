package licenta.facultate.alex.backendlicenta.service;

import jakarta.persistence.EntityManager;
import licenta.facultate.alex.backendlicenta.dto.api.MeDtos;
import licenta.facultate.alex.backendlicenta.dto.api.ShopDtos;
import licenta.facultate.alex.backendlicenta.entity.UserAvatar;
import licenta.facultate.alex.backendlicenta.entity.UserBackground;
import licenta.facultate.alex.backendlicenta.entity.UserBooster;
import licenta.facultate.alex.backendlicenta.entity.UserProfile;
import licenta.facultate.alex.backendlicenta.entity.id.UserBoosterId;
import licenta.facultate.alex.backendlicenta.exception.ApiException;
import licenta.facultate.alex.backendlicenta.repository.UserAvatarRepository;
import licenta.facultate.alex.backendlicenta.repository.UserBackgroundRepository;
import licenta.facultate.alex.backendlicenta.repository.UserBoosterRepository;
import licenta.facultate.alex.backendlicenta.repository.UserProfileRepository;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Map;

@Service
public class ShopService {
    private static final Map<String, Integer> BOOSTER_PRICES = Map.of(
            "booster_double_xp_10m", 800,
            "booster_double_xp_30m", 1500,
            "booster_double_xp_1h", 2800,
            "booster_double_coins_10m", 1000,
            "booster_double_coins_30m", 2900,
            "booster_double_coins_1h", 5700
    );

    private static final Map<String, Integer> BACKGROUND_PRICES = Map.of(
            "desert_background", 400,
            "jungle_background", 800,
            "mountain_background", 1200,
            "ocean_background", 1400
    );

    private static final Map<String, Integer> AVATAR_PRICES = Map.ofEntries(
            Map.entry("hipster_male", 800),
            Map.entry("rocker_male", 1000),
            Map.entry("police_male", 1300),
            Map.entry("doctor_male", 1500),
            Map.entry("firefighter_male", 1800),
            Map.entry("basketball_male", 2400),
            Map.entry("wizard_male", 4600),
            Map.entry("king_male", 6200),
            Map.entry("rocker_female", 800),
            Map.entry("chef_female", 1000),
            Map.entry("tennis_female", 1300),
            Map.entry("witch_female", 1500),
            Map.entry("fairy_female", 1800),
            Map.entry("princess_female", 2400),
            Map.entry("mermaid_female", 4600),
            Map.entry("queen_female", 6200)
    );

    private final UserProfileRepository userProfileRepository;
    private final UserBoosterRepository userBoosterRepository;
    private final UserBackgroundRepository userBackgroundRepository;
    private final UserAvatarRepository userAvatarRepository;
    private final MeService meService;
    private final EntityManager entityManager;
    private final BadgeAwardService badgeAwardService;

    public ShopService(
            UserProfileRepository userProfileRepository,
            UserBoosterRepository userBoosterRepository,
            UserBackgroundRepository userBackgroundRepository,
            UserAvatarRepository userAvatarRepository,
            MeService meService,
            EntityManager entityManager,
            BadgeAwardService badgeAwardService
    ) {
        this.userProfileRepository = userProfileRepository;
        this.userBoosterRepository = userBoosterRepository;
        this.userBackgroundRepository = userBackgroundRepository;
        this.userAvatarRepository = userAvatarRepository;
        this.meService = meService;
        this.entityManager = entityManager;
        this.badgeAwardService = badgeAwardService;
    }

    @Transactional
    public MeDtos.ProfileUpdateResponse purchaseBooster(
            String userId,
            String username,
            ShopDtos.PurchaseBoosterRequest request
    ) {
        UserProfile profile = loadRequiredProfile(userId);
        String boosterId = request.boosterId().trim();

        int price = resolvePrice(
                BOOSTER_PRICES,
                boosterId,
                "UNSUPPORTED_BOOSTER",
                "Booster is not sold in the shop"
        );

        spendCoins(profile, price);

        UserBoosterId boosterIdKey = new UserBoosterId(userId, boosterId);
        UserBooster booster = userBoosterRepository.findById(boosterIdKey).orElse(null);

        if (booster == null) {
            booster = new UserBooster(profile, boosterId, 1);
            profile.addBooster(booster);
            entityManager.persist(booster);
        } else {
            booster.setQuantity(defaultOne(booster.getQuantity()) + 1);
        }

        List<String> awardedBadgeIds = badgeAwardService.awardShopBadges(profile);

        return new MeDtos.ProfileUpdateResponse(
                meService.getProfile(userId, username),
                awardedBadgeIds
        );
    }

    @Transactional
    public MeDtos.ProfileUpdateResponse purchaseBackground(
            String userId,
            String username,
            ShopDtos.PurchaseBackgroundRequest request
    ) {
        UserProfile profile = loadRequiredProfile(userId);
        String backgroundId = request.backgroundId().trim();

        if (userBackgroundRepository.existsById_UserIdAndId_BackgroundId(userId, backgroundId)) {
            throw new ApiException(
                    HttpStatus.CONFLICT,
                    "BACKGROUND_ALREADY_OWNED",
                    "Background is already owned by the current user"
            );
        }

        int price = resolvePrice(
                BACKGROUND_PRICES,
                backgroundId,
                "UNSUPPORTED_BACKGROUND",
                "Background is not sold in the shop"
        );

        spendCoins(profile, price);

        UserBackground background = new UserBackground(profile, backgroundId);
        profile.addBackground(background);
        entityManager.persist(background);

        List<String> awardedBadgeIds = badgeAwardService.awardShopBadges(profile);

        return new MeDtos.ProfileUpdateResponse(
                meService.getProfile(userId, username),
                awardedBadgeIds
        );
    }

    @Transactional
    public MeDtos.ProfileUpdateResponse purchaseAvatar(
            String userId,
            String username,
            ShopDtos.PurchaseAvatarRequest request
    ) {
        UserProfile profile = loadRequiredProfile(userId);
        String avatarId = request.avatarId().trim();

        if (userAvatarRepository.existsById_UserIdAndId_AvatarId(userId, avatarId)) {
            throw new ApiException(
                    HttpStatus.CONFLICT,
                    "AVATAR_ALREADY_OWNED",
                    "Avatar is already owned by the current user"
            );
        }

        int price = resolvePrice(
                AVATAR_PRICES,
                avatarId,
                "UNSUPPORTED_AVATAR",
                "Avatar is not sold in the shop"
        );

        spendCoins(profile, price);

        UserAvatar avatar = new UserAvatar(profile, avatarId);
        profile.addAvatar(avatar);
        entityManager.persist(avatar);

        List<String> awardedBadgeIds = badgeAwardService.awardShopBadges(profile);

        return new MeDtos.ProfileUpdateResponse(
                meService.getProfile(userId, username),
                awardedBadgeIds
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

    private int resolvePrice(
            Map<String, Integer> catalog,
            String itemId,
            String code,
            String message
    ) {
        Integer price = catalog.get(itemId);
        if (price == null) {
            throw new ApiException(HttpStatus.BAD_REQUEST, code, message);
        }
        return price;
    }

    private void spendCoins(UserProfile profile, int price) {
        int currentCoins = defaultZero(profile.getCoins());

        if (currentCoins < price) {
            throw new ApiException(
                    HttpStatus.CONFLICT,
                    "INSUFFICIENT_COINS",
                    "Not enough coins to purchase this item"
            );
        }

        profile.setCoins(currentCoins - price);
        profile.setTotalShopPurchases(defaultZero(profile.getTotalShopPurchases()) + 1);
        profile.setTotalCoinsSpentInShop(defaultZero(profile.getTotalCoinsSpentInShop()) + price);
    }

    private int defaultZero(Integer value) {
        return value == null ? 0 : value;
    }

    private int defaultOne(Integer value) {
        return value == null || value < 1 ? 1 : value;
    }
}
