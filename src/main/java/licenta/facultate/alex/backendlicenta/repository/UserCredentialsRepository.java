package licenta.facultate.alex.backendlicenta.repository;

import licenta.facultate.alex.backendlicenta.entity.UserCredentials;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface UserCredentialsRepository extends JpaRepository<UserCredentials, String> {
    Optional<UserCredentials> findByUsername(String username);
    boolean existsByUsername(String username);
}
