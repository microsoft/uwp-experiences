var vid,
	videoContainer,
	videoControls,
	controlsTimer,
	playPauseBtn,
	seekSlider,
	curTimeText,
	remTimeText,
	aspRatioBtn,
	volumeBtn,
	volumeModal,
	muteBtn,
	volumeSlider,
	fullscreenBtn,
	systemMediaControls;

var timer = 4000; //milliseconds for fade timer


function initializePlayer(){
	//Set high level references
	vid = document.getElementById("myVideo");
	videoContainer = document.getElementById("videoContainer");
	videoControls = document.getElementById("videoControls");
	
	//replace native video controls with custom
	vid.controls = false;
	videoControls.style.display = 'block';
	controlsTimer = setTimeout(function() {
		$('#videoControls').fadeOut('slow');
		$('#volumeModal').fadeOut('slow');
	}, timer);

	//define button references
	playPauseBtn = document.getElementById("playPauseBtn");
	seekSlider = document.getElementById("seekSlider");
	curTimeText = document.getElementById("curTimeText");
	remTimeText = document.getElementById("remTimeText");
	aspRatioBtn = document.getElementById("aspRatioBtn");
	volumeBtn = document.getElementById("volumeBtn");
	volumeModal = document.getElementById("volumeModal");
	muteBtn = document.getElementById("muteBtn");
	volumeSlider = document.getElementById("volumeSlider");
	fullscreenBtn = document.getElementById("fullscreenBtn");

	

	//event listeners for buttons
	vid.addEventListener("mouseover",showControls,false);
	videoControls.addEventListener("mouseover",controlTimerReset,false);
	document.body.addEventListener("keydown",showControls,false);
	playPauseBtn.addEventListener("click",playPause,false);
	seekSlider.addEventListener("change",vidSeek,false);
	vid.addEventListener("timeupdate",seekTimeUpdate,false);

	//volume button & modal handlers
	volumeBtn.addEventListener("click",toggleVolumeModal,false);
	muteBtn.addEventListener("click",vidMute,false);
	volumeSlider.addEventListener("change",setVolume,false);
	window.addEventListener("click",focusOffModal,false);
	
	fullscreenBtn.addEventListener("click",toggleFullscreen,false);

	//Add SMTC support
	if (typeof Windows !== 'undefined') {
  		systemMediaControls = Windows.Media.SystemMediaTransportControls.getForCurrentView();
  		systemMediaControls.addEventListener("buttonpressed", systemMediaControlsButtonPressed, false);
		systemMediaControls.isPlayEnabled = true;
		systemMediaControls.isPauseEnabled = true;
		systemMediaControls.isStopEnabled = true;

		systemMediaControls.playbackStatus = Windows.Media.MediaPlaybackStatus.closed;


	//Hookup SMTC functions
	vid.addEventListener("pause", mediaPaused);
  	vid.addEventListener("playing", mediaPlaying);
  	vid.addEventListener("ended", mediaEnded);
	  	}
}

//SMTC functions
function playMedia() {
  var media = document.getElementById("myVideo");
  media.play();
}
function pauseMedia() {
  var media = document.getElementById("myVideo");
  media.pause();
}
function stopMedia() {
  var media = document.getElementById("myVideo");
  media.pause();
  media.currentTime = 0;
}

function systemMediaControlsButtonPressed(eventIn) {
  var mediaButton = Windows.Media.SystemMediaTransportControlsButton;
  switch (eventIn.button) {
    case mediaButton.play:
      playMedia();
      break;
    case mediaButton.pause:
      pauseMedia();
      break;
    case mediaButton.stop:
      stopMedia();
      break;
  }
}

function mediaPlaying() {
  // Update the SystemMediaTransportControl state.
  systemMediaControls.playbackStatus = Windows.Media.MediaPlaybackStatus.playing;
}
function mediaPaused() {
  // Update the SystemMediaTransportControl state.
  systemMediaControls.playbackStatus = Windows.Media.MediaPlaybackStatus.paused;
}
function mediaEnded() {
  // Update the SystemMediaTransportControl state.
  systemMediaControls.playbackStatus = Windows.Media.MediaPlaybackStatus.stopped;
}


//window.onload = initializePlayer;

function showControls(event) {

	//show controller
	$('#videoControls').fadeIn('slow');
	//reset timer
	controlTimerReset(event);
}

function controlTimerReset(event) {

	//reset timer
	window.clearTimeout(controlsTimer);

	//restart timeout
	controlsTimer = setTimeout(function() {
		$('#videoControls').fadeOut('slow');
		$('#volumeModal').fadeOut('slow');
	}, timer);
}

function playPause(event){
	if(vid.paused){
		vid.play();
		playPauseBtn.innerHTML = "&#xE769;";
	} else {
		vid.pause();
		playPauseBtn.innerHTML = "&#xE768;";
	}
}

function vidSeek() {
	var seekto = vid.duration * (seekSlider.value / 100);
	vid.currentTime = seekto;
}

function seekTimeUpdate() {
	var newTime = vid.currentTime * (100 / vid.duration);
	seekSlider.value = newTime;
	
	var curHours = Math.floor(vid.currentTime / 3600);
	var curMins = Math.floor((vid.currentTime - curHours * 3600) / 60);
	var curSecs = Math.floor(vid.currentTime - curHours * 3600 - curMins * 60);
	
	remTime = vid.duration - vid.currentTime;

	var remHours = Math.floor(remTime / 3600);
	var remMins = Math.floor((remTime - remHours * 3600) / 60);
	var remSecs = Math.floor(remTime - remHours * 3600 - remMins * 60);
	
	if (curSecs < 10) { curSecs = "0" + curSecs; }
	if (curMins < 10) { curMins = "0" + curMins; }
	if (curHours < 10) { curHours = "0" + curHours; }
	if (remSecs < 10) { remSecs = "0" + remSecs; }
	if (remMins < 10) { remMins = "0" + remMins; }
	if (remHours < 10) { remHours = "0" + remHours; }

	curTimeText.innerHTML = curHours + ":" + curMins + ":" + curSecs;
	remTimeText.innerHTML = "-" + remHours + ":" + remMins + ":" + remSecs;

	if (vid.ended) {
		playPauseBtn.innerHTML = "&#xE768;";
		remTimeText.innerHTML = "-00:00:00";
	}
}

function vidMute() {
	if(vid.muted){
		vid.muted = false;
		muteBtn.innerHTML = "&#xE15D";
		volumeBtn.innerHTML = "&#xE15D";

	} else {
		vid.muted = true;
		muteBtn.innerHTML = "&#xE74F";
		volumeBtn.innerHTML = "&#xE74F";
	}
}

function setVolume() {
	vid.volume = volumeSlider.value / 100;
	
	if(vid.muted){
		vid.muted = false;
		muteBtn.innerHTML = "&#xE15D";
		volumeBtn.innerHTML = "&#xE15D";
	}
}

var setFullscreenData = function(state) {
  	videoContainer.setAttribute('datafullscreen', !!state);
}

function toggleFullscreen(){
	var state = !!(document.fullScreen || document.webkitIsFullScreen || document.mozFullScreen || document.msFullscreenElement || document.fullscreenElement);
	
	if(state) { //is in fullscreen --> need to exit FS
		if(document.webkitExitFullscreen) {
			document.webkitExitFullscreen();
		} else if (document.mozCancelFullScreen){
			document.mozCancelFullScreen();
		}
		fullscreenBtn.innerHTML = "&#xE1D9;";
		setFullscreenData(false);

	} else { //not in fullscreen --> need to enter FS
		if (vid.requestFullscreen) {
			vid.requestFullScreen();
		} else if (vid.webkitRequestFullscreen) {
			vid.webkitRequestFullscreen();
		} else if (vid.mozRequestFullscreen) {
			vid.mozRequestFullScreen();
		}
		fullscreenBtn.innerHTML = "&#xE1D8;";
		setFullscreenData(true);
	}
}

function toggleVolumeModal() {
	if (volumeModal.style.display == "block") { //if its open, close it
		$("#volumeModal").fadeOut('slow');
	} else { //if its closed, open it
		renderVolumeModal();
	}
}

function renderVolumeModal() {
	//just to rerender in case sizing changes -- wont need to do this after focus out is there.
	volumeModal.style.display = "none";
	
	$("#volumeModal").fadeIn('slow');
	volumeModal.style.display = "block";
	volumeModal.style.position = "absolute";
	var coords = $("#volumeBtn").offset();
	coords.top = coords.top - volumeModal.offsetHeight - 5;
	coords.left = coords.left - (volumeModal.offsetWidth - volumeBtn.offsetWidth) / 2;
	$("#volumeModal").offset(coords);
}

function focusOffModal(event) {
	if (volumeModal.style.display == "block" 
		&& event.target != volumeModal
		&& event.target != volumeBtn
		&& event.target != muteBtn
		&& event.target != volumeSlider)  { //if its open, close it
		$("#volumeModal").fadeOut('slow');
	}
}