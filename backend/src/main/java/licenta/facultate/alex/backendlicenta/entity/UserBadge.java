package licenta.facultate.alex.backendlicenta.entity;

import jakarta.persistence.*;
import licenta.facultate.alex.backendlicenta.entity.id.UserBadgeId;

import java.time.OffsetDateTime;

@Entity
@Table(name = "USER_BADGES")
public class UserBadge {

    @EmbeddedId
    private UserBadgeId id;

    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @MapsId("userId")
    @JoinColumn(name = "USER_ID", nullable = false)
    private UserProfile profile;

    @Column(name = "UNLOCKED_AT", nullable = false)
    private OffsetDateTime unlockedAt;

    protected UserBadge() {
    }

    public UserBadge(UserProfile profile, String badgeId) {
        this.id = new UserBadgeId(null, badgeId);
        setProfile(profile);
    }

    public UserBadgeId getId() {
        return id;
    }

    public void setId(UserBadgeId id) {
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

    public OffsetDateTime getUnlockedAt() {
        return unlockedAt;
    }

    public void setUnlockedAt(OffsetDateTime unlockedAt) {
        this.unlockedAt = unlockedAt;
    }

    @PrePersist
    void prePersist() {
        if (unlockedAt == null) {
            unlockedAt = OffsetDateTime.now();
        }
    }
}
