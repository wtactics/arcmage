var dieInDOM = [];

function getFace(pips) {
    var XMLNS = "http://www.w3.org/2000/svg";
    var svg = document.createElementNS(XMLNS, 'svg');
    svg.setAttribute('class', 'dice-face');
    svg.setAttribute('width', 32);
    svg.setAttribute('height', 32);

    pips.map(function (pip) {
        var circle = document.createElementNS(XMLNS, 'circle');
        Object.keys(pip).forEach(key => circle.setAttribute(key, pip[key]));
        return circle;
    }).forEach(circle => svg.appendChild(circle));

    return svg;
};

function appendDieContainers(dieId, element, angle) {
    var outer = document.createElement('div');
    outer.className = 'dice-outer';
    outer.id = dieId;
    element.appendChild(outer);

    var dice = document.createElement('div');
    dice.className = 'dice';
    dice.style.transform = `rotateX(${angle[0]}deg) rotateZ(${angle[1]}deg)`;
    outer.appendChild(dice);
    return dice;
}

function removeDieFromDOM(dieId) {
    var removeElement = document.getElementById(dieId);
    if (removeElement) {
        removeElement.remove();
    }
}

var rollADie = function (options) {
    var delay = options.delay || 3000;
    var dotColor = options.dotColor || "#000000";
    if (dieInDOM.length) {
        dieInDOM.forEach(die => removeDieFromDOM(die));
        dieInDOM.length = 0; //reset the array
    }
    var faces = 6;
    var result = [];
   
    for (let i = 0; i < options.numberOfDice; i++) {
        var dieFace = options.values ? options.values[i] : Math.floor(Math.random() * 6) + 1;
        result.push(dieFace);
        var angle = {
            1: [90, 0],
            2: [0, 90],
            3: [180, 0],
            4: [0, 0],
            5: [0, -90],
            6: [-90, 0],
        }[dieFace];
        var dieId = `${Math.random() * 10}-${dieFace}`;
        dieInDOM.push(dieId);
        var dice = appendDieContainers(dieId, options.element, angle);
        [
            [{ cx: 16, cy: 16, r: 3, fill: dotColor}],
            [{ cx: 8, cy: 8, r: 3, fill: dotColor }, { cx: 24, cy: 24, r: 3, fill: dotColor }],
            [{ cx: 8, cy: 8, r: 3, fill: dotColor }, { cx: 16, cy: 16, r: 3, fill: dotColor }, { cx: 24, cy: 24, r: 3, fill: dotColor }],
            [{ cx: 8, cy: 8, r: 3, fill: dotColor }, { cx: 24, cy: 24, r: 3, fill: dotColor }, { cx: 8, cy: 24, r: 3, fill: dotColor }, { cx: 24, cy: 8, r: 3, fill: dotColor }],
            [{ cx: 8, cy: 8, r: 3, fill: dotColor }, { cx: 16, cy: 16, r: 3, fill: dotColor }, { cx: 24, cy: 24, r: 3, fill: dotColor }, { cx: 8, cy: 24, r: 3, fill: dotColor }, { cx: 24, cy: 8, r: 3, fill: dotColor }],
            [{ cx: 8, cy: 8, r: 3, fill: dotColor }, { cx: 24, cy: 24, r: 3, fill: dotColor }, { cx: 8, cy: 16, r: 3, fill: dotColor }, { cx: 24, cy: 16, r: 3, fill: dotColor }, { cx: 8, cy: 24, r: 3, fill: dotColor }, { cx: 24, cy: 8, r: 3, fill: dotColor }]
        ].map(getFace).forEach(face => dice.appendChild(face));

        setTimeout(() => removeDieFromDOM(dieId), delay);

        if (result.length === options.numberOfDice && options.callback) {
            options.callback(result);
        }
    }
};