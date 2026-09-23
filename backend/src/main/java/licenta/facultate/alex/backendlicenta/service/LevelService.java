package licenta.facultate.alex.backendlicenta.service;

import org.springframework.stereotype.Service;

@Service
public class LevelService {
    private static final int BASE_EXPERIENCE = 100;
    private static final double EXPERIENCE_GROWTH = 1.5;

    public int experienceNeededForNextLevel(int currentLevel) {
        int safeLevel = Math.max(1, currentLevel);
        return (int) Math.round(BASE_EXPERIENCE * Math.pow(EXPERIENCE_GROWTH, safeLevel - 1));
    }
}
