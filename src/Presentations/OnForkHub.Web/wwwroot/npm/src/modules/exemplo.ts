// Armazena as instâncias por elemento
const instances = new WeakMap<HTMLElement, ExemploHandler>();

class ExemploHandler {
    private element: HTMLElement;
    private count: number;

    constructor(element: HTMLElement) {
        this.element = element;
        this.count = 0;
        this.init();
    }

    private init() {
        this.updateDisplay();
        this.element.addEventListener('click', () => this.increment());
    }

    increment() {
        this.count++;
        this.updateDisplay();
        console.log('Exemplo incrementado para:', this.count);
    }

    private updateDisplay() {
        this.element.innerHTML = `Exemplo count: ${this.count}`;
    }
}

function setupExemplo(element: HTMLElement): ExemploHandler {
    // Cria e armazena a instância
    const instance = new ExemploHandler(element);
    instances.set(element, instance);
    return instance;
}

function incrementExemplo(element: HTMLElement) {
    // Usa a instância existente ou cria uma nova
    let instance = instances.get(element);
    if (!instance) {
        instance = new ExemploHandler(element);
        instances.set(element, instance);
    }
    instance.increment();
}

export {
    setupExemplo as Exemplo,
    incrementExemplo
}