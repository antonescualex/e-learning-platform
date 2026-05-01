package licenta.facultate.alex.backendlicenta.repository;

import licenta.facultate.alex.backendlicenta.entity.UserBackground;
import licenta.facultate.alex.backendlicenta.entity.id.UserBackgroundId;
import org.springframework.data.jpa.repository.JpaRepository;

public interface UserBackgroundRepository extends JpaRepository<UserBackground, UserBackgroundId> {
}
