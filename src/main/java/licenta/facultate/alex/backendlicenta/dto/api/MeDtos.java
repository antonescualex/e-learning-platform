package licenta.facultate.alex.backendlicenta.dto.api;

import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;

import java.time.LocalDate;
import java.time.OffsetDateTime;
import java.util.List;

public final class MeDtos {
    private MeDtos() {
    }

    public record BoosterDto(
            @JsonProperty("BoosterItemId") String boosterItemId,
            @JsonProperty("Quantity") int quantity
    ) {
    }

    public record ProfileResponse(
            @JsonProperty("UserId") String userId,
            @JsonProperty("Username") String username,
            @JsonProperty("PlayerName") String playerName,
            @JsonProperty("LevelNumber") int levelNumber,
            @JsonProperty("CurrentExperience") int currentExperience,
            @JsonProperty("ExperienceNeededForNextLevel") int experienceNeededForNextLevel,
            @JsonProperty("Coins") int coins,
            @JsonProperty("SelectedAvatarId") String selectedAvatarId,
            @JsonProperty("SelectedBackgroundId") String selectedBackgroundId,
            @JsonProperty("CreatedAt") OffsetDateTime createdAt,
            @JsonProperty("LastLoginDate") LocalDate lastLoginDate,
            @JsonProperty("CurrentLoginStreak") int currentLoginStreak,
            @JsonProperty("CompletedLessonsCount") int completedLessonsCount,
            @JsonProperty("IncompleteLessonsCount") int incompleteLessonsCount,
            @JsonProperty("TotalShopPurchases") int totalShopPurchases,
            @JsonProperty("TotalCoinsSpentInShop") int totalCoinsSpentInShop,
            @JsonProperty("DoubleCoinsExpiresAt") OffsetDateTime doubleCoinsExpiresAt,
            @JsonProperty("DoubleXpExpiresAt") OffsetDateTime doubleXpExpiresAt,
            @JsonProperty("OwnedBadgeIds") List<String> ownedBadgeIds,
            @JsonProperty("OwnedAvatarIds") List<String> ownedAvatarIds,
            @JsonProperty("OwnedBackgroundIds") List<String> ownedBackgroundIds,
            @JsonProperty("Boosters") List<BoosterDto> boosters
    ) {
    }

    public record ProfileUpdateResponse(
            @JsonProperty("Profile") ProfileResponse profile,
            @JsonProperty("AwardedBadgeIds") List<String> awardedBadgeIds
    ) {
    }

    public record UpdateProfileRequest(
            @JsonProperty("PlayerName")
            @NotBlank
            @Size(min = 3, max = 10)
            String playerName
    ) {
    }

    public record SelectAvatarRequest(
            @JsonProperty("AvatarId")
            @NotBlank
            @Size(max = 100)
            String avatarId
    ) {
    }

    public record SelectBackgroundRequest(
            @JsonProperty("BackgroundId")
            @NotBlank
            @Size(max = 100)
            String backgroundId
    ) {
    }

    public record ActivateBoosterRequest(
            @JsonProperty("BoosterItemId")
            @NotBlank
            @Size(max = 100)
            String boosterItemId
    ) {
    }
}
