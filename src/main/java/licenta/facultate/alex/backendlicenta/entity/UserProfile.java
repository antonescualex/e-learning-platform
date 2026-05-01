package licenta.facultate.alex.backendlicenta.entity;

import jakarta.persistence.*;

import java.time.LocalDate;
import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.List;

@Entity
@Table(name = "USER_PROFILE")
public class UserProfile {

    @Id
    @Column(name = "USER_ID", nullable = false, length = 36)
    private String userId;

    @OneToOne(fetch = FetchType.LAZY, optional = false)
    @MapsId
    @JoinColumn(name = "USER_ID", nullable = false)
    private UserCredentials credentials;

    @Column(name = "PLAYER_NAME", nullable = false, length = 100)
    private String playerName;

    @Column(name = "LEVEL_NUMBER", nullable = false)
    private Integer levelNumber = 1;

    @Column(name = "CURRENT_EXPERIENCE", nullable = false)
    private Integer currentExperience = 0;

    @Column(name = "COINS", nullable = false)
    private Integer coins = 0;

    @Column(name = "SELECTED_AVATAR_ID", nullable = false, length = 100)
    private String selectedAvatarId = "boy_3";

    @Column(name = "SELECTED_BACKGROUND_ID", nullable = false, length = 100)
    private String selectedBackgroundId = "default_background";

    @Column(name = "CREATED_AT", nullable = false)
    private OffsetDateTime createdAt;

    @Column(name = "LAST_LOGIN_DATE", nullable = false)
    private LocalDate lastLoginDate = LocalDate.now();

    @Column(name = "CURRENT_LOGIN_STREAK", nullable = false)
    private Integer currentLoginStreak = 1;

    @Column(name = "COMPLETED_LESSONS_COUNT", nullable = false)
    private Integer completedLessonsCount = 0;

    @Column(name = "INCOMPLETE_LESSONS_COUNT", nullable = false)
    private Integer incompleteLessonsCount = 0;

    @Column(name = "TOTAL_SHOP_PURCHASES", nullable = false)
    private Integer totalShopPurchases = 0;

    @Column(name = "TOTAL_COINS_SPENT_IN_SHOP", nullable = false)
    private Integer totalCoinsSpentInShop = 0;

    @Column(name = "DOUBLE_COINS_EXPIRES_AT")
    private OffsetDateTime doubleCoinsExpiresAt;

    @Column(name = "DOUBLE_XP_EXPIRES_AT")
    private OffsetDateTime doubleXpExpiresAt;

    @Column(name = "UPDATED_AT", nullable = false)
    private OffsetDateTime updatedAt;

    @OneToMany(mappedBy = "profile", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<UserBadge> badges = new ArrayList<>();

    @OneToMany(mappedBy = "profile", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<UserAvatar> avatars = new ArrayList<>();

    @OneToMany(mappedBy = "profile", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<UserBackground> backgrounds = new ArrayList<>();

    @OneToMany(mappedBy = "profile", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<UserBooster> boosters = new ArrayList<>();

    protected UserProfile() {
    }

    public UserProfile(UserCredentials credentials, String playerName) {
        this.credentials = credentials;
        this.playerName = playerName;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }

    public UserCredentials getCredentials() {
        return credentials;
    }

    public void setCredentials(UserCredentials credentials) {
        this.credentials = credentials;
    }

    public String getPlayerName() {
        return playerName;
    }

    public void setPlayerName(String playerName) {
        this.playerName = playerName;
    }

    public Integer getLevelNumber() {
        return levelNumber;
    }

    public void setLevelNumber(Integer levelNumber) {
        this.levelNumber = levelNumber;
    }

    public Integer getCurrentExperience() {
        return currentExperience;
    }

    public void setCurrentExperience(Integer currentExperience) {
        this.currentExperience = currentExperience;
    }

    public Integer getCoins() {
        return coins;
    }

    public void setCoins(Integer coins) {
        this.coins = coins;
    }

    public String getSelectedAvatarId() {
        return selectedAvatarId;
    }

    public void setSelectedAvatarId(String selectedAvatarId) {
        this.selectedAvatarId = selectedAvatarId;
    }

    public String getSelectedBackgroundId() {
        return selectedBackgroundId;
    }

    public void setSelectedBackgroundId(String selectedBackgroundId) {
        this.selectedBackgroundId = selectedBackgroundId;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(OffsetDateTime createdAt) {
        this.createdAt = createdAt;
    }

    public LocalDate getLastLoginDate() {
        return lastLoginDate;
    }

    public void setLastLoginDate(LocalDate lastLoginDate) {
        this.lastLoginDate = lastLoginDate;
    }

    public Integer getCurrentLoginStreak() {
        return currentLoginStreak;
    }

    public void setCurrentLoginStreak(Integer currentLoginStreak) {
        this.currentLoginStreak = currentLoginStreak;
    }

    public Integer getCompletedLessonsCount() {
        return completedLessonsCount;
    }

    public void setCompletedLessonsCount(Integer completedLessonsCount) {
        this.completedLessonsCount = completedLessonsCount;
    }

    public Integer getIncompleteLessonsCount() {
        return incompleteLessonsCount;
    }

    public void setIncompleteLessonsCount(Integer incompleteLessonsCount) {
        this.incompleteLessonsCount = incompleteLessonsCount;
    }

    public Integer getTotalShopPurchases() {
        return totalShopPurchases;
    }

    public void setTotalShopPurchases(Integer totalShopPurchases) {
        this.totalShopPurchases = totalShopPurchases;
    }

    public Integer getTotalCoinsSpentInShop() {
        return totalCoinsSpentInShop;
    }

    public void setTotalCoinsSpentInShop(Integer totalCoinsSpentInShop) {
        this.totalCoinsSpentInShop = totalCoinsSpentInShop;
    }

    public OffsetDateTime getDoubleCoinsExpiresAt() {
        return doubleCoinsExpiresAt;
    }

    public void setDoubleCoinsExpiresAt(OffsetDateTime doubleCoinsExpiresAt) {
        this.doubleCoinsExpiresAt = doubleCoinsExpiresAt;
    }

    public OffsetDateTime getDoubleXpExpiresAt() {
        return doubleXpExpiresAt;
    }

    public void setDoubleXpExpiresAt(OffsetDateTime doubleXpExpiresAt) {
        this.doubleXpExpiresAt = doubleXpExpiresAt;
    }

    public OffsetDateTime getUpdatedAt() {
        return updatedAt;
    }

    public void setUpdatedAt(OffsetDateTime updatedAt) {
        this.updatedAt = updatedAt;
    }

    public List<UserBadge> getBadges() {
        return badges;
    }

    public void setBadges(List<UserBadge> badges) {
        this.badges = badges;
    }

    public List<UserAvatar> getAvatars() {
        return avatars;
    }

    public void setAvatars(List<UserAvatar> avatars) {
        this.avatars = avatars;
    }

    public List<UserBackground> getBackgrounds() {
        return backgrounds;
    }

    public void setBackgrounds(List<UserBackground> backgrounds) {
        this.backgrounds = backgrounds;
    }

    public List<UserBooster> getBoosters() {
        return boosters;
    }

    public void setBoosters(List<UserBooster> boosters) {
        this.boosters = boosters;
    }

    @PrePersist
    void prePersist() {
        OffsetDateTime now = OffsetDateTime.now();
        if (createdAt == null) {
            createdAt = now;
        }
        if (updatedAt == null) {
            updatedAt = now;
        }
        if (lastLoginDate == null) {
            lastLoginDate = LocalDate.now();
        }
        if (levelNumber == null) {
            levelNumber = 1;
        }
        if (currentExperience == null) {
            currentExperience = 0;
        }
        if (coins == null) {
            coins = 0;
        }
        if (selectedAvatarId == null) {
            selectedAvatarId = "boy_3";
        }
        if (selectedBackgroundId == null) {
            selectedBackgroundId = "default_background";
        }
        if (currentLoginStreak == null) {
            currentLoginStreak = 1;
        }
        if (completedLessonsCount == null) {
            completedLessonsCount = 0;
        }
        if (incompleteLessonsCount == null) {
            incompleteLessonsCount = 0;
        }
        if (totalShopPurchases == null) {
            totalShopPurchases = 0;
        }
        if (totalCoinsSpentInShop == null) {
            totalCoinsSpentInShop = 0;
        }
    }

    @PreUpdate
    void preUpdate() {
        updatedAt = OffsetDateTime.now();
    }
}
