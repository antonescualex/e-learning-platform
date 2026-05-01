package licenta.facultate.alex.backendlicenta.repository;

import licenta.facultate.alex.backendlicenta.entity.UserAvatar;
import licenta.facultate.alex.backendlicenta.entity.id.UserAvatarId;
import org.springframework.data.jpa.repository.JpaRepository;

public interface UserAvatarRepository extends JpaRepository<UserAvatar, UserAvatarId> {
}
