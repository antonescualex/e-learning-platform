package licenta.facultate.alex.backendlicenta.entity.id;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;

import java.io.Serializable;
import java.util.Objects;

@Embeddable
public class UserAvatarId implements Serializable {

    @Column(name = "USER_ID", nullable = false, length = 36)
    private String userId;

    @Column(name = "AVATAR_ID", nullable = false, length = 100)
    private String avatarId;

    protected UserAvatarId() {
    }

    public UserAvatarId(String userId, String avatarId) {
        this.userId = userId;
        this.avatarId = avatarId;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }

    public String getAvatarId() {
        return avatarId;
    }

    public void setAvatarId(String avatarId) {
        this.avatarId = avatarId;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof UserAvatarId that)) return false;
        return Objects.equals(userId, that.userId) && Objects.equals(avatarId, that.avatarId);
    }

    @Override
    public int hashCode() {
        return Objects.hash(userId, avatarId);
    }
}
