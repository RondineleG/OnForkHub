function setupCounter(element: HTMLElement) {
    console.log("setupCounter - Iniciando com elemento:", element);
    let counter = 0;
    const setCounter = (count: number) => {
        counter = count;
        element.innerHTML = `count is ${counter}`;
        console.log("Counter atualizado para:", counter);
    };
    element.addEventListener('click', () => {
        console.log("Clique detectado - valor atual:", counter);
        setCounter(counter + 1);
    });
    setCounter(0);
}

function incrementCounter(element: HTMLElement) {
    const currentCount = parseInt(element.innerHTML.replace('count is ', ''), 10) || 0;
    element.innerHTML = `count is ${currentCount + 1}`;
    console.log("Counter incrementado para:", currentCount + 1);
}

function helloWorld() {
    console.log('Hello, World - from JS');
}

export {
    setupCounter as Counter,
    helloWorld as HelloWorld,
    incrementCounter
}