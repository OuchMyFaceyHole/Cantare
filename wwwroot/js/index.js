let guessCount = 1;
let enumValToColor = {
    0: "green",
    1: "orange",
    2: "yellow",
    3: "magenta",
    4: "red",
    5: "dimgrey"
}
let audioInterval = null;
let audioData = null;
let audioContext = new AudioContext();
let gainSetter = null;
let isPlaying = false;
$(document).ready(async function () {
    audioData = await (await fetch('/GetSongAudioData')).arrayBuffer();
    
    gainSetter = audioContext.createGain();
    gainSetter.gain.value = 0.25;
    var progressBar = document.getElementById("PlayProgress");
    var songInputGroup = document.getElementById("SongInputGroup");
    var guessContainer = document.getElementById("GuessContainer");
    document.addEventListener('click', async function (event) {
        if (event.target.id === "PlayButton" && !isPlaying) {
            await PlaySong();
        }
        else if (event.target.id === "SubmitButton") {
            let songInput = document.getElementById("SongInput");
            if (workingSet.includes(songInput.dataset.trueValue)) {
                $.ajax({
                    url: '/SongGuess',
                    type: 'POST',
                    data: { songGuess: songInput.dataset.trueValue },
                    success: async function (data) {
                        if (data != 0) {
                            var currentGuessRow = guessContainer.children[guessCount - 1];
                            currentGuessRow.style.backgroundColor = enumValToColor[data];
                            currentGuessRow.firstElementChild.firstElementChild.innerHTML = songInput.value;
                            currentGuessRow.firstElementChild.firstElementChild.style.margin = "0.5rem";
                            currentGuessRow.firstElementChild.firstElementChild.hidden = false;
                            songInput.value = "";
                            progressBar.style.width = (100 / 6) * guessCount + "%";
                            if (guessCount < 6) {
                                guessCount++;
                                guessContainer.children[guessCount - 1].children[0].children[0].hidden = true;
                                guessContainer.children[guessCount - 1].children[0].appendChild(songInputGroup);
                                guessContainer.children[guessCount - 1].children[0].style.backgroundColor = "";
                            }
                            else {
                                songInputGroup.hidden = true;
                            }
                        }
                        else {
                            var currentGuessRow = guessContainer.children[guessCount - 1];
                            currentGuessRow.style.backgroundColor = enumValToColor[data];
                            currentGuessRow.firstElementChild.firstElementChild.innerHTML = songInput.value;
                            currentGuessRow.firstElementChild.firstElementChild.style.margin = "0.5rem";
                            currentGuessRow.firstElementChild.firstElementChild.hidden = false;
                            currentGuessRow.firstElementChild.hidden = false;
                            songInputGroup.hidden = true;
                            guessCount = 6;
                            progressBar.style.width = "100%";
                        }
                        await PlaySong();
                    }
                });
                workingSet = [];
            }
            else {
                ElementError(songInput);
            }
        }
        else if (event.target.id === "SkipButton") {
            var currentGuessRow = guessContainer.children[guessCount - 1];
            currentGuessRow.style.backgroundColor = enumValToColor[5];
            currentGuessRow.firstElementChild.firstElementChild.innerHTML = "Skipped";
            currentGuessRow.firstElementChild.firstElementChild.style.margin = "0.5rem";
            currentGuessRow.firstElementChild.firstElementChild.hidden = false;
            progressBar.style.width = (100 / 6) * guessCount + "%";
            if (guessCount < 6) {
                guessCount++;
                guessContainer.children[guessCount - 1].children[0].children[0].hidden = true;
                guessContainer.children[guessCount - 1].children[0].appendChild(songInputGroup);
                guessContainer.children[guessCount - 1].children[0].style.backgroundColor = "";
            }
            else {
                songInputGroup.hidden = true;
            }
        }
    });
    const modal = document.getElementById("modal");
    modal.addEventListener('show.bs.modal', event => {
        if (event.relatedTarget.getAttribute('data-type') == 'calendar') {
            $.ajax({
                url: '/GetCalendarData',
                type: 'GET',
                data: {page: 0},
                success: function (data) {
                    document.getElementById("Calendar").innerHTML = data;
                }
            });
        }
    });
});

function ElementError(element) {
    element.classList.add("ahashakeheartache");
    element.style.outlineColor = "red";
    element.style.outlineStyle = "solid";
    setTimeout(() => {
        element.classList.remove("ahashakeheartache");
        element.style.outlineColor = "";
        element.style.outlineStyle = "";
    }, 500);
}

async function PlaySong() {
    let audioBuffer = await audioContext.decodeAudioData(audioData.slice(0));
    audioSource = audioContext.createBufferSource();
    audioSource.addEventListener("ended", () => {
        isPlaying = false;
    });
    audioSource.buffer = audioBuffer;
    audioSource.connect(gainSetter).connect(audioContext.destination);
    audioSource.start(0, 0, guessCount);
    isPlaying = true;
}