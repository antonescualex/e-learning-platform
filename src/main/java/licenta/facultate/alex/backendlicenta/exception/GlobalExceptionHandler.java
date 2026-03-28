package licenta.facultate.alex.backendlicenta.exception;

import licenta.facultate.alex.backendlicenta.dto.api.LessonContentDtos;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.http.converter.HttpMessageNotReadableException;
import org.springframework.validation.FieldError;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import java.util.List;
import java.util.UUID;

@RestControllerAdvice
public class GlobalExceptionHandler {
    @ExceptionHandler(ApiException.class)
    public ResponseEntity<LessonContentDtos.ApiErrorResponse> handleApi(ApiException ex) {
        return ResponseEntity.status(ex.status()).body(body(ex.code(), ex.getMessage(), ex.details()));
    }

    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ResponseEntity<LessonContentDtos.ApiErrorResponse> handleValidation(MethodArgumentNotValidException ex) {
        List<String> details = ex.getBindingResult().getFieldErrors().stream()
                .map(this::fieldMessage)
                .toList();

        return ResponseEntity.badRequest().body(body("VALIDATION_ERROR", "Request validation failed", details));
    }

    @ExceptionHandler(HttpMessageNotReadableException.class)
    public ResponseEntity<LessonContentDtos.ApiErrorResponse> handleJson(HttpMessageNotReadableException ex) {
        return ResponseEntity.badRequest().body(body("INVALID_JSON", "Malformed JSON request", List.of(ex.getMostSpecificCause().getMessage())));
    }

    @ExceptionHandler(Exception.class)
    public ResponseEntity<LessonContentDtos.ApiErrorResponse> handleUnexpected(Exception ex) {
        return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                .body(body("INTERNAL_ERROR", "Unexpected server error", List.of()));
    }

    private LessonContentDtos.ApiErrorResponse body(String code, String message, List<String> details) {
        return new LessonContentDtos.ApiErrorResponse(code, message, details, UUID.randomUUID().toString());
    }

    private String fieldMessage(FieldError error) {
        return error.getField() + ": " + (error.getDefaultMessage() == null ? "invalid value" : error.getDefaultMessage());
    }
}
