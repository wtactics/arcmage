import { Subscription } from "rxjs";

export class SubscriptionManager {
    subscriptions: { [key: string]: Subscription } = {};

    // Key is required in case of overwrites / duplicate adds
    // Subscriptions cannot be properly compared
    add(key: string, subscription: Subscription) {
        this.remove(key);
        this.subscriptions[key] = subscription;
    }

    remove(key: string) {
        if (!this.subscriptions[key]) {
            return;
        }

        this.subscriptions[key].unsubscribe();
        delete this.subscriptions[key];
    }

    clear() {
        const subscriptions = this.subscriptions;
        for (const key in subscriptions) {
            if (subscriptions.hasOwnProperty(key)) {
                this.remove(key);
            }
        }
    }
}
