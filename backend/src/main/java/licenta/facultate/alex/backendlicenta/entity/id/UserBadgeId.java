package licenta.facultate.alex.backendlicenta.entity.id;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;

import java.io.Serializable;
import java.util.Objects;

@Embeddable
public class UserBadgeId implements Serializable {

    @Column(name = "USER_ID", nullable = false, length = 36)
    private String userId;

    @Column(name = "BADGE_ID", nullable = false, length = 100)
    private String badgeId;

    protected UserBadgeId() {
    }

    public UserBadgeId(String userId, String badgeId) {
        this.userId = userId;
        this.badgeId = badgeId;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }

    public String getBadgeId() {
        return badgeId;
    }

    public void setBadgeId(String badgeId) {
        this.badgeId = badgeId;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof UserBadgeId that)) return false;
        return Objects.equals(userId, that.userId) && Objects.equals(badgeId, that.badgeId);
    }

    @Override
    public int hashCode() {
        return Objects.hash(userId, badgeId);
    }
}
