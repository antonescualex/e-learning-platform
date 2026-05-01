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
    }

    public OffsetDateTime getUnlockedAt() {
        return unlockedAt;
    }

    public void setUnlockedAt(OffsetDateTime unlockedAt) {
        this.unlockedAt = unlockedAt;
    }

    public UserBadge(UserProfile profile, String badgeId) {
        this.profile = profile;
        this.id = new UserBadgeId(null, badgeId);
    }

    @PrePersist
    void prePersist() {
        if (unlockedAt == null) {
            unlockedAt = OffsetDateTime.now();
        }
    }
}
