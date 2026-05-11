package licenta.facultate.alex.backendlicenta.repository;

import licenta.facultate.alex.backendlicenta.entity.UserBooster;
import licenta.facultate.alex.backendlicenta.entity.id.UserBoosterId;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface UserBoosterRepository extends JpaRepository<UserBooster, UserBoosterId> {
    List<UserBooster> findAllById_UserIdOrderByUpdatedAtAsc(String userId);
}
