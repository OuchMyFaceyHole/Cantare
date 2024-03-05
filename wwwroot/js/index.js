$(document).ready(async function () {
    let audioContext = new AudioContext();
    let audioBuffer = await audioContext.decodeAudioData(await (await fetch('/GetSongData')).arrayBuffer());
    let audioSource = audioContext.createBufferSource();
    audioSource.buffer = audioBuffer;
    let gainSetter = audioContext.createGain();
    gainSetter.gain.value = 0.25;
    audioSource.connect(gainSetter).connect(audioContext.destination);
    document.addEventListener('click', function (event) {
        if (event.target.id == "PlayButton") {
            audioSource.start();
        }
    });
});