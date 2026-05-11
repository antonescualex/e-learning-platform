package licenta.facultate.alex.backendlicenta.dto.api;

import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;

public final class ShopDtos {
    private ShopDtos() {
    }

    public record PurchaseBoosterRequest(
            @JsonProperty("BoosterId")
            @NotBlank
            @Size(max = 100)
            String boosterId
    ) {
    }

    public record PurchaseBackgroundRequest(
            @JsonProperty("BackgroundId")
            @NotBlank
            @Size(max = 100)
            String backgroundId
    ) {
    }

    public record PurchaseAvatarRequest(
            @JsonProperty("AvatarId")
            @NotBlank
            @Size(max = 100)
            String avatarId
    ) {
    }
}
