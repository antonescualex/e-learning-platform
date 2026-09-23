package licenta.facultate.alex.backendlicenta.config;

import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.info.Info;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class OpenApiConfig {

    @Bean
    public OpenAPI lessonContentApi() {
        return new OpenAPI()
                .info(new Info()
                        .title("Lesson Content API")
                        .version("v1")
                        .description("Backend between Unity and OpenAI Responses API"));
    }
}