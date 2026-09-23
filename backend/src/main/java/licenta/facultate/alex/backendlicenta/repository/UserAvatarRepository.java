package licenta.facultate.alex.backendlicenta.repository;

import licenta.facultate.alex.backendlicenta.entity.UserAvatar;
import licenta.facultate.alex.backendlicenta.entity.id.UserAvatarId;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface UserAvatarRepository extends JpaRepository<UserAvatar, UserAvatarId> {
    List<UserAvatar> findAllById_UserIdOrderByAcquiredAtAsc(String userId);
    boolean existsById_UserIdAndId_AvatarId(String userId, String avatarId);
}
