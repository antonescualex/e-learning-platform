package licenta.facultate.alex.backendlicenta.controller;

import jakarta.validation.Valid;
import licenta.facultate.alex.backendlicenta.dto.api.AuthDtos;
import licenta.facultate.alex.backendlicenta.service.AuthService;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/auth")
public class AuthController {
    private final AuthService authService;

    public AuthController(AuthService authService) {
        this.authService = authService;
    }

    @PostMapping("/register")
    public AuthDtos.RegisterResponse register(@Valid @RequestBody AuthDtos.RegisterRequest request) {
        return authService.register(request);
    }
}
