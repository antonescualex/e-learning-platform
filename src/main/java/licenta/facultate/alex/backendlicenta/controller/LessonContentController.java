package licenta.facultate.alex.backendlicenta.controller;

import jakarta.validation.Valid;
import licenta.facultate.alex.backendlicenta.dto.api.LessonContentDtos;
import licenta.facultate.alex.backendlicenta.service.LessonContentService;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/lesson-content")
public class LessonContentController {

    private final LessonContentService lessonContentService;

    public LessonContentController(LessonContentService lessonContentService) {
        this.lessonContentService = lessonContentService;
    }

    @PostMapping("/text-choice")
    public LessonContentDtos.TextChoiceLessonResponse textChoice(@Valid @RequestBody LessonContentDtos.LessonRequest request) {
        return lessonContentService.generateTextChoice(request);
    }

    @PostMapping("/shapes")
    public LessonContentDtos.ShapesLessonResponse shapes(@Valid @RequestBody LessonContentDtos.ShapesLessonRequest request) {
        return lessonContentService.generateShapes(request);
    }

    @PostMapping("/clock")
    public LessonContentDtos.ClockLessonResponse clock(@Valid @RequestBody LessonContentDtos.LessonRequest request) {
        return lessonContentService.generateClock(request);
    }

    @PostMapping("/syllable-division")
    public LessonContentDtos.SyllableDivisionLessonResponse syllableDivision(@Valid @RequestBody LessonContentDtos.LessonRequest request) {
        return lessonContentService.generateSyllableDivision(request);
    }

    @PostMapping("/write-correctly")
    public LessonContentDtos.WriteCorrectlyLessonResponse writeCorrectly(@Valid @RequestBody LessonContentDtos.LessonRequest request) {
        return lessonContentService.generateWriteCorrectly(request);
    }
}
