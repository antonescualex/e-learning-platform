package licenta.facultate.alex.backendlicenta.service;

import licenta.facultate.alex.backendlicenta.dto.api.LessonProgressDtos;
import licenta.facultate.alex.backendlicenta.dto.api.MeDtos;
import licenta.facultate.alex.backendlicenta.entity.UserProfile;
import licenta.facultate.alex.backendlicenta.exception.ApiException;
import licenta.facultate.alex.backendlicenta.model.LessonId;
import licenta.facultate.alex.backendlicenta.repository.UserProfileRepository;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.OffsetDateTime;
import java.util.List;

@Service
public class LessonProgressService {

    private final UserProfileRepository userProfileRepository;
    private final MeService meService;
    private final BadgeAwardService badgeAwardService;
    private final LevelService levelService;

    public LessonProgressService(
            UserProfileRepository userProfileRepository,
            MeService meService,
            BadgeAwardService badgeAwardService,
            LevelService levelService
    ) {
        this.userProfileRepository = userProfileRepository;
        this.meService = meService;
        this.badgeAwardService = badgeAwardService;
        this.levelService = levelService;
    }

    @Transactional
    public MeDtos.ProfileUpdateResponse completeLesson(
            String userId,
            String username,
            LessonProgressDtos.CompleteLessonRequest request
    ) {
        LessonId.fromClientValue(request.lessonId().trim());

        if (request.correctAnswersCount() > request.questionCount()) {
            throw new ApiException(
                    HttpStatus.BAD_REQUEST,
                    "INVALID_SCORE",
                    "CorrectAnswersCount cannot be greater than QuestionCount"
            );
        }

        UserProfile profile = userProfileRepository.findById(userId)
                .orElseThrow(() -> new ApiException(
                        HttpStatus.INTERNAL_SERVER_ERROR,
                        "PROFILE_MISSING",
                        "User profile is missing"
                ));

        if (request.completed()) {
            profile.setCompletedLessonsCount(defaultZero(profile.getCompletedLessonsCount()) + 1);
        } else {
            profile.setIncompleteLessonsCount(defaultZero(profile.getIncompleteLessonsCount()) + 1);
        }

        profile.setCoins(defaultZero(profile.getCoins()) + request.awardedCoins());
        profile.setCurrentExperience(defaultZero(profile.getCurrentExperience()) + request.awardedExperience());

        applyLevelUp(profile);

        List<String> awardedBadgeIds = badgeAwardService.awardLessonBadges(
                profile,
                request.completed(),
                request.elapsedSeconds(),
                request.correctAnswersCount(),
                request.questionCount()
        );

        return new MeDtos.ProfileUpdateResponse(
                meService.getProfile(userId, username),
                awardedBadgeIds
        );
    }

    private void applyLevelUp(UserProfile profile) {
        int levelNumber = defaultLevel(profile.getLevelNumber());
        int currentExperience = defaultZero(profile.getCurrentExperience());

        while (currentExperience >= levelService.experienceNeededForNextLevel(levelNumber)) {
            currentExperience -= levelService.experienceNeededForNextLevel(levelNumber);
            levelNumber++;
        }

        profile.setLevelNumber(levelNumber);
        profile.setCurrentExperience(currentExperience);
    }

    private int defaultZero(Integer value) {
        return value == null ? 0 : value;
    }

    private int defaultLevel(Integer value) {
        return value == null || value < 1 ? 1 : value;
    }
}