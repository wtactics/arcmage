import { Duration, isDuration } from "moment";
import * as moment from "moment";

export class DurationUtils {
    static fromDate(date: Date): Duration {
        if (!date || !(date instanceof Date)) {
            return null;
        }

        const hours = date.getHours();
        const minutes = date.getMinutes();
        const seconds = date.getSeconds();
        return moment.duration().add(hours, "hours").add(minutes, "minutes").add(seconds, "seconds");
    }

    static toDate(duration: Duration): Date {
        if (!duration) {
            return null;
        }

        const date = new Date();
        date.setHours(duration.hours());
        date.setMinutes(duration.minutes());
        date.setSeconds(duration.seconds());
        return date;
    }

    static toString(duration: Duration | Date): string {
        if (duration instanceof Date) {
            duration = DurationUtils.fromDate(duration);
        }

        if (!isDuration(duration)) {
            return duration;
        }

        return moment.utc(duration.as("milliseconds")).format("HH:mm:ss");
    }
}
