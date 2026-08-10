mergeInto(LibraryManager.library, {
  BNPointerLock_Install: function () {
    var canvas = Module['canvas'];
    if (!canvas) {
      return 0;
    }

    var state = Module['breakNoticePointerLock'];
    if (!state) {
      state = {
        canvas: canvas,
        enabled: false,
        installed: false,
        pending: false,
        error: false,
        fallback: false,
        fallbackDeltaX: 0,
        fallbackDeltaY: 0,
        pendingTimer: 0,
        originalCursor: canvas.style.cursor
      };
      Module['breakNoticePointerLock'] = state;
    }

    if (!state.installed) {
      state.clearPendingTimer = function () {
        if (state.pendingTimer) {
          window.clearTimeout(state.pendingTimer);
          state.pendingTimer = 0;
        }
      };

      state.setFallback = function (active) {
        state.fallback = active === true;
        state.fallbackDeltaX = 0;
        state.fallbackDeltaY = 0;
        canvas.style.cursor = state.fallback ? 'none' : state.originalCursor;
      };

      state.failIntoFallback = function () {
        state.clearPendingTimer();
        state.pending = false;
        state.error = true;
        state.setFallback(true);
      };

      state.onClick = function (event) {
        if (!state.enabled ||
            event.isTrusted === false ||
            event.button !== 0 ||
            state.pending ||
            state.fallback ||
            document.pointerLockElement) {
          return;
        }

        state.pending = true;
        state.error = false;
        state.setFallback(false);

        try {
          if (typeof canvas.focus === 'function') {
            canvas.focus();
          }

          var request = canvas.requestPointerLock();
          if (request && typeof request.catch === 'function') {
            request.catch(function () {
              state.failIntoFallback();
            });
          }

          state.pendingTimer = window.setTimeout(function () {
            if (state.pending && document.pointerLockElement !== canvas) {
              state.failIntoFallback();
            }
          }, 600);
        } catch (_) {
          state.failIntoFallback();
        }
      };

      state.onMouseMove = function (event) {
        if (!state.enabled || !state.fallback) {
          return;
        }

        state.fallbackDeltaX += event.movementX || 0;
        // Unity mouse delta uses positive Y upward; DOM movementY is downward.
        state.fallbackDeltaY -= event.movementY || 0;
      };

      state.onPointerLockChange = function () {
        state.clearPendingTimer();
        state.pending = false;
        if (document.pointerLockElement === canvas) {
          state.error = false;
          state.setFallback(false);
        } else {
          state.setFallback(false);
        }
      };

      state.onPointerLockError = function () {
        if (state.pending) {
          state.failIntoFallback();
        }
      };

      // The primary click runs after the browser's normal focus transition.
      // Requesting in pointerdown capture can race an embedded Chromium host
      // that has not made the document active yet.
      canvas.addEventListener('click', state.onClick, false);
      canvas.addEventListener('mousemove', state.onMouseMove, false);
      document.addEventListener('pointerlockchange', state.onPointerLockChange, false);
      document.addEventListener('pointerlockerror', state.onPointerLockError, false);
      canvas.addEventListener('contextmenu', function (event) {
        event.preventDefault();
      });
      state.installed = true;
    }

    return typeof canvas.requestPointerLock === 'function' ? 1 : 0;
  },

  BNPointerLock_SetEnabled: function (enabled) {
    var state = Module['breakNoticePointerLock'];
    if (state) {
      state.enabled = enabled !== 0;
      if (!state.enabled) {
        state.clearPendingTimer();
        state.pending = false;
        state.error = false;
        state.setFallback(false);
      }
    }
  },

  BNPointerLock_HasError: function () {
    var state = Module['breakNoticePointerLock'];
    return state && state.error ? 1 : 0;
  },

  BNPointerLock_IsPending: function () {
    var state = Module['breakNoticePointerLock'];
    return state && state.pending ? 1 : 0;
  },

  BNPointerLock_IsFallbackActive: function () {
    var state = Module['breakNoticePointerLock'];
    return state && state.fallback ? 1 : 0;
  },

  BNPointerLock_ConsumeFallbackDeltaX: function () {
    var state = Module['breakNoticePointerLock'];
    if (!state) {
      return 0;
    }

    var value = state.fallbackDeltaX | 0;
    state.fallbackDeltaX = 0;
    return value;
  },

  BNPointerLock_ConsumeFallbackDeltaY: function () {
    var state = Module['breakNoticePointerLock'];
    if (!state) {
      return 0;
    }

    var value = state.fallbackDeltaY | 0;
    state.fallbackDeltaY = 0;
    return value;
  },

  BNPointerLock_IsSupported: function () {
    var canvas = Module['canvas'];
    return canvas && typeof canvas.requestPointerLock === 'function' ? 1 : 0;
  },

  BNPointerLock_IsLocked: function () {
    return document.pointerLockElement === Module['canvas'] ? 1 : 0;
  },

  BNPointerLock_Exit: function () {
    var state = Module['breakNoticePointerLock'];
    if (state) {
      state.clearPendingTimer();
      state.pending = false;
      state.error = false;
      state.setFallback(false);
    }

    var canvas = Module['canvas'];
    if (document.pointerLockElement === canvas && document.exitPointerLock) {
      document.exitPointerLock();
    }
  }
});
