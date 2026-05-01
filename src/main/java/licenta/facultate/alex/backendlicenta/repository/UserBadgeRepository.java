package licenta.facultate.alex.backendlicenta.repository;

import licenta.facultate.alex.backendlicenta.entity.UserBadge;
import licenta.facultate.alex.backendlicenta.entity.id.UserBadgeId;
import org.springframework.data.jpa.repository.JpaRepository;

public interface UserBadgeRepository extends JpaRepository<UserBadge, UserBadgeId> {
}
