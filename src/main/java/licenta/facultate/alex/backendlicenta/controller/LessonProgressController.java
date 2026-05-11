package licenta.facultate.alex.backendlicenta.controller;

import jakarta.validation.Valid;
import licenta.facultate.alex.backendlicenta.dto.api.LessonProgressDtos;
import licenta.facultate.alex.backendlicenta.dto.api.MeDtos;
import licenta.facultate.alex.backendlicenta.service.LessonProgressService;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/lessons")
public class LessonProgressController {
    private final LessonProgressService lessonProgressService;

    public LessonProgressController(LessonProgressService lessonProgressService) {
        this.lessonProgressService = lessonProgressService;
    }

    @PostMapping("/complete")
    public MeDtos.ProfileUpdateResponse complete(
            @AuthenticationPrincipal Jwt jwt,
            @Valid @RequestBody LessonProgressDtos.CompleteLessonRequest request
    ) {
        String userId = jwt.getSubject();
        String username = jwt.getClaimAsString("username");

        return lessonProgressService.completeLesson(userId, username, request);
    }
}