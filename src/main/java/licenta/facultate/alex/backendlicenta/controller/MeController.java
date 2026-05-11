package licenta.facultate.alex.backendlicenta.controller;

import jakarta.validation.Valid;
import licenta.facultate.alex.backendlicenta.dto.api.MeDtos;
import licenta.facultate.alex.backendlicenta.service.MeService;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/me")
public class MeController {

    private final MeService meService;

    public MeController(MeService meService) {
        this.meService = meService;
    }

    @GetMapping("/profile")
    public MeDtos.ProfileResponse profile(@AuthenticationPrincipal Jwt jwt) {
        String userId = jwt.getSubject();
        String username = jwt.getClaimAsString("username");

        return meService.getProfile(userId, username);
    }

    @PatchMapping("/profile")
    public MeDtos.ProfileResponse updateProfile(
            @AuthenticationPrincipal Jwt jwt,
            @Valid @RequestBody MeDtos.UpdateProfileRequest request
    ) {
        String userId = jwt.getSubject();
        String username = jwt.getClaimAsString("username");

        return meService.updateProfile(userId, username, request);
    }

    @PostMapping("/select-avatar")
    public MeDtos.ProfileResponse selectAvatar(
            @AuthenticationPrincipal Jwt jwt,
            @Valid @RequestBody MeDtos.SelectAvatarRequest request
    ) {
        String userId = jwt.getSubject();
        String username = jwt.getClaimAsString("username");

        return meService.selectAvatar(userId, username, request);
    }

    @PostMapping("/select-background")
    public MeDtos.ProfileResponse selectBackground(
            @AuthenticationPrincipal Jwt jwt,
            @Valid @RequestBody MeDtos.SelectBackgroundRequest request
    ) {
        String userId = jwt.getSubject();
        String username = jwt.getClaimAsString("username");

        return meService.selectBackground(userId, username, request);
    }

    @PostMapping("/daily-login")
    public MeDtos.ProfileUpdateResponse dailyLogin(@AuthenticationPrincipal Jwt jwt) {
        String userId = jwt.getSubject();
        String username = jwt.getClaimAsString("username");

        return meService.dailyLogin(userId, username);
    }

    @PostMapping("/activate-booster")
    public MeDtos.ProfileResponse activateBooster(
            @AuthenticationPrincipal Jwt jwt,
            @Valid @RequestBody MeDtos.ActivateBoosterRequest request
    ) {
        String userId = jwt.getSubject();
        String username = jwt.getClaimAsString("username");

        return meService.activateBooster(userId, username, request);
    }
}