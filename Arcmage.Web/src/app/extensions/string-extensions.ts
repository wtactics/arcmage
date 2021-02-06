interface String {
    contains(value: string): boolean;
    replaceAll(search: string, replacement: string): string;
    toCamelCase(): string;
    toPascalCase(): string;
}

String.prototype.contains = function(value) {
    return this.toLowerCase().indexOf(value.toLowerCase()) > -1;
};

String.prototype.replaceAll = function (search, replacement) {
    return this.replace(new RegExp(search, "g"), replacement);
};

String.prototype.toCamelCase = function() {
    if (this.length < 1) {
        return this;
    }

    return this.substr(0, 1).toLowerCase() + this.substr(1);
};

String.prototype.toPascalCase = function() {
    if (this.length < 1) {
        return this;
    }

    return this.substr(0, 1).toUpperCase() + this.substr(1);
};