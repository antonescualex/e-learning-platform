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
    private final UserBadgeRepository userBadgeRepository;
    private final EntityManager entityManager;

    public BadgeAwardService(UserBadgeRepository userBadgeRepository, EntityManager entityManager) {
        this.userBadgeRepository = userBadgeRepository;
        this.entityManager = entityManager;
    }

    public List<String> awardDailyBadges(UserProfile profile) {
        int streak = defaultZero(profile.getCurrentLoginStreak());
        List<String> awarded = new ArrayList<>();

        awardIf(profile, awarded, "badge_first_day", streak >= 1);
        awardIf(profile, awarded, "badge_on_fire_1", streak >= 3);
        awardIf(profile, awarded, "badge_on_fire_2", streak >= 7);
        awardIf(profile, awarded, "badge_addicted", streak >= 30);

        return awarded;
    }

    public List<String> awardLessonBadges(
            UserProfile profile,
            boolean completed,
            int elapsedSeconds,
            int correctAnswers,
            int totalQuestions
    ) {
        List<String> awarded = new ArrayList<>();

        if (completed) {
            awardIf(profile, awarded, "badge_speed_runner_1", elapsedSeconds < 60);
            awardIf(profile, awarded, "badge_speed_runner_2", elapsedSeconds < 40);
            awardIf(profile, awarded, "badge_speed_runner_3", elapsedSeconds < 30);
            awardIf(profile, awarded, "badge_lightning", elapsedSeconds < 20);

            awardIf(profile, awarded, "badge_perfect_score", totalQuestions > 0 && correctAnswers == totalQuestions);
        }

        int completedLessons = defaultZero(profile.getCompletedLessonsCount());
        awardIf(profile, awarded, "badge_first_steps", completedLessons >= 1);
        awardIf(profile, awarded, "badge_learner_1", completedLessons >= 5);
        awardIf(profile, awarded, "badge_learner_2", completedLessons >= 20);
        awardIf(profile, awarded, "badge_learner_3", completedLessons >= 50);
        awardIf(profile, awarded, "badge_master_learner", completedLessons >= 100);

        return awarded;
    }

    public List<String> awardShopBadges(UserProfile profile) {
        int totalPurchases = defaultZero(profile.getTotalShopPurchases());
        int totalCoinsSpent = defaultZero(profile.getTotalCoinsSpentInShop());
        List<String> awarded = new ArrayList<>();

        awardIf(profile, awarded, "badge_first_purchase", totalPurchases >= 1);
        awardIf(profile, awarded, "badge_collector", totalPurchases >= 5);
        awardIf(profile, awarded, "badge_shop_addict", totalPurchases >= 20);
        awardIf(profile, awarded, "badge_big_spender", totalCoinsSpent >= 1000);

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
