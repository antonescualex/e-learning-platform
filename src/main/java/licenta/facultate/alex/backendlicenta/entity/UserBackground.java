package licenta.facultate.alex.backendlicenta.entity;

import jakarta.persistence.*;
import licenta.facultate.alex.backendlicenta.entity.id.UserBackgroundId;

import java.time.OffsetDateTime;

@Entity
@Table(name = "USER_BACKGROUNDS")
public class UserBackground {

    @EmbeddedId
    private UserBackgroundId id;

    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @MapsId("userId")
    @JoinColumn(name = "USER_ID", nullable = false)
    private UserProfile profile;

    @Column(name = "ACQUIRED_AT", nullable = false)
    private OffsetDateTime acquiredAt;

    protected UserBackground() {
    }

    public UserBackground(UserProfile profile, String backgroundId) {
        this.id = new UserBackgroundId(null, backgroundId);
        setProfile(profile);
    }

    public UserBackgroundId getId() {
        return id;
    }

    public void setId(UserBackgroundId id) {
        this.id = id;
    }

    public UserProfile getProfile() {
        return profile;
    }

    public void setProfile(UserProfile profile) {
        this.profile = profile;
        if (this.id != null) {
            this.id.setUserId(profile == null ? null : profile.getUserId());
        }
    }

    public OffsetDateTime getAcquiredAt() {
        return acquiredAt;
    }

    public void setAcquiredAt(OffsetDateTime acquiredAt) {
        this.acquiredAt = acquiredAt;
    }

    @PrePersist
    void prePersist() {
        if (acquiredAt == null) {
            acquiredAt = OffsetDateTime.now();
        }
    }
}
