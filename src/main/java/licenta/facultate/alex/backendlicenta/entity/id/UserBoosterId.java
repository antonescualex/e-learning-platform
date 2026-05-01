package licenta.facultate.alex.backendlicenta.entity.id;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;

import java.io.Serializable;
import java.util.Objects;

@Embeddable
public class UserBoosterId implements Serializable {

    @Column(name = "USER_ID", nullable = false, length = 36)
    private String userId;

    @Column(name = "BOOSTER_ITEM_ID", nullable = false, length = 100)
    private String boosterItemId;

    protected UserBoosterId() {
    }

    public UserBoosterId(String userId, String boosterItemId) {
        this.userId = userId;
        this.boosterItemId = boosterItemId;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }

    public String getBoosterItemId() {
        return boosterItemId;
    }

    public void setBoosterItemId(String boosterItemId) {
        this.boosterItemId = boosterItemId;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof UserBoosterId that)) return false;
        return Objects.equals(userId, that.userId) && Objects.equals(boosterItemId, that.boosterItemId);
    }

    @Override
    public int hashCode() {
        return Objects.hash(userId, boosterItemId);
    }
}
