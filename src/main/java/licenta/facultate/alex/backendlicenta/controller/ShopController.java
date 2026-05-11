package licenta.facultate.alex.backendlicenta.controller;

import jakarta.validation.Valid;
import licenta.facultate.alex.backendlicenta.dto.api.MeDtos;
import licenta.facultate.alex.backendlicenta.dto.api.ShopDtos;
import licenta.facultate.alex.backendlicenta.service.ShopService;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/shop")
public class ShopController {
    private final ShopService shopService;

    public ShopController(ShopService shopService) {
        this.shopService = shopService;
    }

    @PostMapping("/purchase-booster")
    public MeDtos.ProfileUpdateResponse purchaseBooster(
            @AuthenticationPrincipal Jwt jwt,
            @Valid @RequestBody ShopDtos.PurchaseBoosterRequest request
    ) {
        String userId = jwt.getSubject();
        String username = jwt.getClaimAsString("username");

        return shopService.purchaseBooster(userId, username, request);
    }

    @PostMapping("/purchase-background")
    public MeDtos.ProfileUpdateResponse purchaseBackground(
            @AuthenticationPrincipal Jwt jwt,
            @Valid @RequestBody ShopDtos.PurchaseBackgroundRequest request
    ) {
        String userId = jwt.getSubject();
        String username = jwt.getClaimAsString("username");

        return shopService.purchaseBackground(userId, username, request);
    }

    @PostMapping("/purchase-avatar")
    public MeDtos.ProfileUpdateResponse purchaseAvatar(
            @AuthenticationPrincipal Jwt jwt,
            @Valid @RequestBody ShopDtos.PurchaseAvatarRequest request
    ) {
        String userId = jwt.getSubject();
        String username = jwt.getClaimAsString("username");

        return shopService.purchaseAvatar(userId, username, request);
    }
}
