package licenta.facultate.alex.backendlicenta.repository;

import licenta.facultate.alex.backendlicenta.entity.UserBooster;
import licenta.facultate.alex.backendlicenta.entity.id.UserBoosterId;
import org.springframework.data.jpa.repository.JpaRepository;

public interface UserBoosterRepository extends JpaRepository<UserBooster, UserBoosterId> {
}
