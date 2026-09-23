package licenta.facultate.alex.backendlicenta.entity.id;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;

import java.io.Serializable;
import java.util.Objects;

@Embeddable
public class UserBackgroundId implements Serializable {

    @Column(name = "USER_ID", nullable = false, length = 36)
    private String userId;

    @Column(name = "BACKGROUND_ID", nullable = false, length = 100)
    private String backgroundId;

    protected UserBackgroundId() {
    }

    public UserBackgroundId(String userId, String backgroundId) {
        this.userId = userId;
        this.backgroundId = backgroundId;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }

    public String getBackgroundId() {
        return backgroundId;
    }

    public void setBackgroundId(String backgroundId) {
        this.backgroundId = backgroundId;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof UserBackgroundId that)) return false;
        return Objects.equals(userId, that.userId) && Objects.equals(backgroundId, that.backgroundId);
    }

    @Override
    public int hashCode() {
        return Objects.hash(userId, backgroundId);
    }
}
