// Mood emoji visual effects
window.moodEffects = {
    // Create and display mood effect based on selected emoji
    showEffect: function (emoji, position) {
        // Get center position from the provided coordinates
        const centerX = position.clientX;
        const centerY = position.clientY;

        // Determine which effect to show
        switch (emoji) {
            case '😭': // Crying - Heavy rain with drops
                this.createRainEffect(centerX, centerY, true);
                break;
            case '😢': // Sad - Light rain
                this.createRainEffect(centerX, centerY, false);
                break;
            case '😕': // Confused - Floating clouds
                this.createCloudEffect(centerX, centerY);
                break;
            case '🤔': // Thinking - Also clouds
                this.createCloudEffect(centerX, centerY);
                break;
            case '😐': // Neutral - No effect
                break;
            case '🙂': // Slight smile - Sparkles
                this.createSparkleEffect(centerX, centerY);
                break;
            case '😀': // Happy - Sunshine with warm glow
                this.createSunshineEffect(centerX, centerY);
                break;
            case '🤩': // Excited - Twinkling stars
                this.createStarEffect(centerX, centerY);
                break;
        }
    },

    // Rain effect (heavy or light)
    createRainEffect: function (x, y, isHeavy) {
        const container = this.createContainer();
        const particleCount = isHeavy ? 30 : 15;
        const emoji = '💧';

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'mood-effect-particle rain-drop';
            particle.textContent = emoji;
            
            // Random position across entire viewport width
            const posX = Math.random() * window.innerWidth;
            const delay = Math.random() * 200;
            
            particle.style.left = posX + 'px';
            particle.style.top = '-20px';
            particle.style.animationDelay = delay + 'ms';
            
            container.appendChild(particle);
        }

        // Add rain lines for crying effect
        if (isHeavy) {
            for (let i = 0; i < 12; i++) {
                const line = document.createElement('div');
                line.className = 'mood-effect-particle rain-line';
                const posX = Math.random() * window.innerWidth;
                line.style.left = posX + 'px';
                line.style.top = '-30px';
                line.style.animationDelay = (Math.random() * 250) + 'ms';
                container.appendChild(line);
            }
        }

        this.autoRemove(container, 600);
    },

    // Cloud effect for confused/thinking
    createCloudEffect: function (x, y) {
        const container = this.createContainer();
        const particleCount = 8;
        const emoji = '☁️';

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'mood-effect-particle cloud';
            particle.textContent = emoji;
            
            // Random position across viewport
            const posX = Math.random() * window.innerWidth;
            const posY = Math.random() * window.innerHeight;
            
            particle.style.left = posX + 'px';
            particle.style.top = posY + 'px';
            particle.style.animationDelay = (i * 75) + 'ms';
            
            container.appendChild(particle);
        }

        this.autoRemove(container, 600);
    },

    // Sparkle effect for slight smile
    createSparkleEffect: function (x, y) {
        const container = this.createContainer();
        const particleCount = 25;
        const sparkles = ['✨', '⭐', '💫'];

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'mood-effect-particle sparkle';
            particle.textContent = sparkles[Math.floor(Math.random() * sparkles.length)];
            
            // Random position across viewport
            const posX = Math.random() * window.innerWidth;
            const posY = Math.random() * window.innerHeight;
            
            particle.style.left = posX + 'px';
            particle.style.top = posY + 'px';
            particle.style.animationDelay = (Math.random() * 200) + 'ms';
            
            container.appendChild(particle);
        }

        this.autoRemove(container, 600);
    },

    // Sunshine effect for happy
    createSunshineEffect: function (x, y) {
        const container = this.createContainer();
        
        // Center of viewport
        const centerX = window.innerWidth / 2;
        const centerY = window.innerHeight / 2;
        
        // Central sun
        const sun = document.createElement('div');
        sun.className = 'mood-effect-particle sun-center';
        sun.textContent = '☀️';
        sun.style.left = centerX + 'px';
        sun.style.top = centerY + 'px';
        container.appendChild(sun);

        // Warm glow rays extending across screen
        const rayCount = 12;
        for (let i = 0; i < rayCount; i++) {
            const ray = document.createElement('div');
            ray.className = 'mood-effect-particle sun-ray';
            const angle = (Math.PI * 2 * i) / rayCount;
            const startDist = 40;
            const offsetX = Math.cos(angle) * startDist;
            const offsetY = Math.sin(angle) * startDist;
            
            ray.style.left = (centerX + offsetX) + 'px';
            ray.style.top = (centerY + offsetY) + 'px';
            ray.style.setProperty('--ray-angle', angle + 'rad');
            
            container.appendChild(ray);
        }

        this.autoRemove(container, 600);
    },

    // Star effect for excited
    createStarEffect: function (x, y) {
        const container = this.createContainer();
        const particleCount = 30;
        const emoji = '⭐';

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'mood-effect-particle star';
            particle.textContent = emoji;
            
            // Random position across viewport
            const posX = Math.random() * window.innerWidth;
            const posY = Math.random() * window.innerHeight;
            
            particle.style.left = posX + 'px';
            particle.style.top = posY + 'px';
            particle.style.animationDelay = (Math.random() * 250) + 'ms';
            
            container.appendChild(particle);
        }

        this.autoRemove(container, 600);
    },

    // Helper to create effect container
    createContainer: function () {
        const container = document.createElement('div');
        container.className = 'mood-effects-container';
        document.body.appendChild(container);
        return container;
    },

    // Helper to auto-remove container after animation
    autoRemove: function (container, duration) {
        setTimeout(() => {
            container.remove();
        }, duration);
    }
};
