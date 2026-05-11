package licenta.facultate.alex.backendlicenta.repository;

import licenta.facultate.alex.backendlicenta.entity.UserBadge;
import licenta.facultate.alex.backendlicenta.entity.id.UserBadgeId;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface UserBadgeRepository extends JpaRepository<UserBadge, UserBadgeId> {
    List<UserBadge> findAllById_UserIdOrderByUnlockedAtAsc(String userId);
    boolean existsById_UserIdAndId_BadgeId(String userId, String badgeId);
}
