export class JsonUtils {

    /**
       * Detect a date string and convert it to a date object.
       * @private
       * @param {*} key json property key.
       * @param {*} value json property value.
       * @returns {*} original value or the parsed date.
       * @memberof JsonInterceptor
       */
    public static reviveUtcDate(key: any, value: any): any {
        if (typeof value !== 'string') {
            return value;
        }
        if (value === '0001-01-01T00:00:00') {
            return null;
        }
        const isoDateRegEx = /^(\d{4})-(\d{2})-(\d{2})(?:[T ](\d{2}):(\d{2})(?::(\d{2})(?:\.(\d*))?)?)?(?:[Z+-]?(-?\d{0,2})?:?(-?\d{0,2}))?$/;
        const match = isoDateRegEx.exec(value);
        if (!match) {
            return value;
        }
        return new Date(value);
    }

    public static parse(text: string): any {
        return JSON.parse(text, (key: any, value: any) => this.reviveUtcDate(key, value));
    }

    public static deepClone<T>(object: T): T {
        const clonedObject = this.parse(JSON.stringify(object));

        return clonedObject;
    }

    public static clone<T>(object: T): T {
        return Object.assign({}, object);
    }
}
