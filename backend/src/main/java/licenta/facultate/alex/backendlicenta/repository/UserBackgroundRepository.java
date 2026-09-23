package licenta.facultate.alex.backendlicenta.repository;

import licenta.facultate.alex.backendlicenta.entity.UserBackground;
import licenta.facultate.alex.backendlicenta.entity.id.UserBackgroundId;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface UserBackgroundRepository extends JpaRepository<UserBackground, UserBackgroundId> {
    List<UserBackground> findAllById_UserIdOrderByAcquiredAtAsc(String userId);
    boolean existsById_UserIdAndId_BackgroundId(String userId, String backgroundId);
}
