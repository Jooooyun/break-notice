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
        error: false
      };
      Module['breakNoticePointerLock'] = state;
    }

    if (!state.installed) {
      state.onPointerDown = function (event) {
        if (!state.enabled ||
            event.isTrusted === false ||
            (event.pointerType && event.pointerType !== 'mouse') ||
            (event.button !== 0 && event.button !== 2) ||
            document.pointerLockElement) {
          return;
        }

        state.pending = true;
        state.error = false;

        try {
          if (typeof canvas.focus === 'function') {
            canvas.focus();
          }

          var request = canvas.requestPointerLock();
          if (request && typeof request.catch === 'function') {
            request.catch(function () {
              state.pending = false;
              state.error = true;
            });
          }
        } catch (_) {
          state.pending = false;
          state.error = true;
        }
      };

      state.onPointerLockChange = function () {
        state.pending = false;
        if (document.pointerLockElement === canvas) {
          state.error = false;
        }
      };

      state.onPointerLockError = function () {
        if (state.pending) {
          state.error = true;
        }
        state.pending = false;
      };

      canvas.addEventListener('pointerdown', state.onPointerDown, true);
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
    }
  },

  BNPointerLock_HasError: function () {
    var state = Module['breakNoticePointerLock'];
    return state && state.error ? 1 : 0;
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
      state.pending = false;
    }

    var canvas = Module['canvas'];
    if (document.pointerLockElement === canvas && document.exitPointerLock) {
      document.exitPointerLock();
    }
  }
});
