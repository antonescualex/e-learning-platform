package licenta.facultate.alex.backendlicenta.entity;

import jakarta.persistence.*;
import licenta.facultate.alex.backendlicenta.entity.id.UserBoosterId;

import java.time.OffsetDateTime;

@Entity
@Table(name = "USER_BOOSTERS")
public class UserBooster {

    @EmbeddedId
    private UserBoosterId id;

    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @MapsId("userId")
    @JoinColumn(name = "USER_ID", nullable = false)
    private UserProfile profile;

    @Column(name = "QUANTITY", nullable = false)
    private Integer quantity = 1;

    @Column(name = "UPDATED_AT", nullable = false)
    private OffsetDateTime updatedAt;

    protected UserBooster() {
    }

    public UserBooster(UserProfile profile, String boosterItemId, Integer quantity) {
        this.id = new UserBoosterId(null, boosterItemId);
        setProfile(profile);
        this.quantity = quantity;
    }

    public UserBoosterId getId() {
        return id;
    }

    public void setId(UserBoosterId id) {
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

    public Integer getQuantity() {
        return quantity;
    }

    public void setQuantity(Integer quantity) {
        this.quantity = quantity;
    }

    public OffsetDateTime getUpdatedAt() {
        return updatedAt;
    }

    public void setUpdatedAt(OffsetDateTime updatedAt) {
        this.updatedAt = updatedAt;
    }

    @PrePersist
    void prePersist() {
        if (quantity == null || quantity < 1) {
            quantity = 1;
        }
        if (updatedAt == null) {
            updatedAt = OffsetDateTime.now();
        }
    }

    @PreUpdate
    void preUpdate() {
        updatedAt = OffsetDateTime.now();
    }
}
