package licenta.facultate.alex.backendlicenta.service;

import jakarta.persistence.EntityManager;
import licenta.facultate.alex.backendlicenta.entity.UserBadge;
import licenta.facultate.alex.backendlicenta.entity.UserProfile;
import licenta.facultate.alex.backendlicenta.repository.UserBadgeRepository;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;

@Service
public class BadgeAwardService {
    private static final String BADGE_FIRST_DAY_ID = "badge_first_day";
    private static final String BADGE_ON_FIRE_1_ID = "badge_on_fire_1";
    private static final String BADGE_ON_FIRE_2_ID = "badge_on_fire_2";
    private static final String BADGE_ADDICTED_ID = "badge_addicted";

    private static final String BADGE_FIRST_PURCHASE_ID = "badge_first_purchase";
    private static final String BADGE_COLLECTOR_ID = "badge_collector";
    private static final String BADGE_SHOP_ADDICT_ID = "badge_shop_addict";
    private static final String BADGE_BIG_SPENDER_ID = "badge_big_spender";

    private static final String BADGE_SPEED_RUNNER_1_ID = "badge_speed_runner_1";
    private static final String BADGE_SPEED_RUNNER_2_ID = "badge_speed_runner_2";
    private static final String BADGE_SPEED_RUNNER_3_ID = "badge_speed_runner_3";
    private static final String BADGE_LIGHTNING_ID = "badge_lightning";
    private static final String BADGE_PERFECT_SCORE_ID = "badge_perfect_score";

    private static final String BADGE_FIRST_STEPS_ID = "badge_first_steps";
    private static final String BADGE_LEARNER_1_ID = "badge_learner_1";
    private static final String BADGE_LEARNER_2_ID = "badge_learner_2";
    private static final String BADGE_LEARNER_3_ID = "badge_learner_3";
    private static final String BADGE_MASTER_LEARNER_ID = "badge_master_learner";

    public static final int ONE_DAY = 1;
    public static final int THREE_DAYS = 3;
    public static final int SEVEN_DAYS = 7;
    public static final int THIRTY_DAYS = 30;

    public static final int SIXTY_SECONDS = 60;
    public static final int FORTY_SECONDS = 40;
    public static final int THIRTY_SECONDS = 30;
    public static final int TWENTY_SECONDS = 20;
    public static final int ZERO_QUESTIONS = 0;

    public static final int ONE_LESSON = 1;
    public static final int FIVE_LESSONS = 5;
    public static final int TWENTY_LESSONS = 20;
    public static final int FIFTY_LESSONS = 50;
    public static final int HUNDRED_LESSONS = 100;

    public static final int ONE_PURCHASE = 1;
    public static final int FIVE_PURCHASES = 5;
    public static final int TWENTY_PURCHASES = 20;
    public static final int COINS_SPENT_THRESHOLD = 1000;

    private final UserBadgeRepository userBadgeRepository;
    private final EntityManager entityManager;

    public BadgeAwardService(UserBadgeRepository userBadgeRepository, EntityManager entityManager) {
        this.userBadgeRepository = userBadgeRepository;
        this.entityManager = entityManager;
    }

    public List<String> awardDailyBadges(UserProfile profile) {
        int streak = defaultZero(profile.getCurrentLoginStreak());
        List<String> awarded = new ArrayList<>();

        awardIf(profile, awarded, BADGE_FIRST_DAY_ID, streak >= ONE_DAY);
        awardIf(profile, awarded, BADGE_ON_FIRE_1_ID, streak >= THREE_DAYS);
        awardIf(profile, awarded, BADGE_ON_FIRE_2_ID, streak >= SEVEN_DAYS);
        awardIf(profile, awarded, BADGE_ADDICTED_ID, streak >= THIRTY_DAYS);

        return awarded;
    }

    public List<String> awardLessonBadges(UserProfile profile, boolean completed, int elapsedSeconds,
            int correctAnswers, int totalQuestions) {
        List<String> awarded = new ArrayList<>();

        if (completed) {
            awardIf(profile, awarded, BADGE_SPEED_RUNNER_1_ID, elapsedSeconds < SIXTY_SECONDS);
            awardIf(profile, awarded, BADGE_SPEED_RUNNER_2_ID, elapsedSeconds < FORTY_SECONDS);
            awardIf(profile, awarded, BADGE_SPEED_RUNNER_3_ID, elapsedSeconds < THIRTY_SECONDS);
            awardIf(profile, awarded, BADGE_LIGHTNING_ID, elapsedSeconds < TWENTY_SECONDS);

            awardIf(profile, awarded, BADGE_PERFECT_SCORE_ID, totalQuestions > ZERO_QUESTIONS && correctAnswers == totalQuestions);
        }

        int completedLessons = defaultZero(profile.getCompletedLessonsCount());
        awardIf(profile, awarded, BADGE_FIRST_STEPS_ID, completedLessons >= ONE_LESSON);
        awardIf(profile, awarded, BADGE_LEARNER_1_ID, completedLessons >= FIVE_LESSONS);
        awardIf(profile, awarded, BADGE_LEARNER_2_ID, completedLessons >= TWENTY_LESSONS);
        awardIf(profile, awarded, BADGE_LEARNER_3_ID, completedLessons >= FIFTY_LESSONS);
        awardIf(profile, awarded, BADGE_MASTER_LEARNER_ID, completedLessons >= HUNDRED_LESSONS);

        return awarded;
    }

    public List<String> awardShopBadges(UserProfile profile) {
        int totalPurchases = defaultZero(profile.getTotalShopPurchases());
        int totalCoinsSpent = defaultZero(profile.getTotalCoinsSpentInShop());
        List<String> awarded = new ArrayList<>();

        awardIf(profile, awarded, BADGE_FIRST_PURCHASE_ID, totalPurchases >= ONE_PURCHASE);
        awardIf(profile, awarded, BADGE_COLLECTOR_ID, totalPurchases >= FIVE_PURCHASES);
        awardIf(profile, awarded, BADGE_SHOP_ADDICT_ID, totalPurchases >= TWENTY_PURCHASES);
        awardIf(profile, awarded, BADGE_BIG_SPENDER_ID, totalCoinsSpent >= COINS_SPENT_THRESHOLD);

        return awarded;
    }

    private void awardIf(UserProfile profile, List<String> awarded, String badgeId, boolean condition) {
        if (condition && awardBadge(profile, badgeId)) {
            awarded.add(badgeId);
        }
    }

    private boolean awardBadge(UserProfile profile, String badgeId) {
        String userId = profile.getUserId();

        if (userBadgeRepository.existsById_UserIdAndId_BadgeId(userId, badgeId)) {
            return false;
        }

        UserBadge badge = new UserBadge(profile, badgeId);
        profile.addBadge(badge);
        entityManager.persist(badge);
        return true;
    }

    private int defaultZero(Integer value) {
        return value == null ? 0 : value;
    }
}
