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
        const particleCount = isHeavy ? 20 : 10;
        const emoji = '💧';

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'mood-effect-particle rain-drop';
            particle.textContent = emoji;
            
            // Random horizontal offset
            const offsetX = (Math.random() - 0.5) * 150;
            const delay = Math.random() * 100;
            
            particle.style.left = (x + offsetX) + 'px';
            particle.style.top = (y - 20) + 'px';
            particle.style.animationDelay = delay + 'ms';
            
            container.appendChild(particle);
        }

        // Add rain lines for crying effect
        if (isHeavy) {
            for (let i = 0; i < 8; i++) {
                const line = document.createElement('div');
                line.className = 'mood-effect-particle rain-line';
                const offsetX = (Math.random() - 0.5) * 120;
                line.style.left = (x + offsetX) + 'px';
                line.style.top = (y - 30) + 'px';
                line.style.animationDelay = (Math.random() * 150) + 'ms';
                container.appendChild(line);
            }
        }

        this.autoRemove(container, 350);
    },

    // Cloud effect for confused/thinking
    createCloudEffect: function (x, y) {
        const container = this.createContainer();
        const particleCount = 5;
        const emoji = '☁️';

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'mood-effect-particle cloud';
            particle.textContent = emoji;
            
            const angle = (Math.PI * 2 * i) / particleCount;
            const radius = 40 + Math.random() * 30;
            const offsetX = Math.cos(angle) * radius;
            const offsetY = Math.sin(angle) * radius;
            
            particle.style.left = (x + offsetX) + 'px';
            particle.style.top = (y + offsetY) + 'px';
            particle.style.animationDelay = (i * 50) + 'ms';
            
            container.appendChild(particle);
        }

        this.autoRemove(container, 350);
    },

    // Sparkle effect for slight smile
    createSparkleEffect: function (x, y) {
        const container = this.createContainer();
        const particleCount = 15;
        const sparkles = ['✨', '⭐', '💫'];

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'mood-effect-particle sparkle';
            particle.textContent = sparkles[Math.floor(Math.random() * sparkles.length)];
            
            const angle = Math.random() * Math.PI * 2;
            const distance = 30 + Math.random() * 70;
            const offsetX = Math.cos(angle) * distance;
            const offsetY = Math.sin(angle) * distance;
            
            particle.style.left = (x + offsetX) + 'px';
            particle.style.top = (y + offsetY) + 'px';
            particle.style.animationDelay = (Math.random() * 100) + 'ms';
            
            container.appendChild(particle);
        }

        this.autoRemove(container, 350);
    },

    // Sunshine effect for happy
    createSunshineEffect: function (x, y) {
        const container = this.createContainer();
        
        // Central sun
        const sun = document.createElement('div');
        sun.className = 'mood-effect-particle sun-center';
        sun.textContent = '☀️';
        sun.style.left = x + 'px';
        sun.style.top = y + 'px';
        container.appendChild(sun);

        // Warm glow rays
        const rayCount = 8;
        for (let i = 0; i < rayCount; i++) {
            const ray = document.createElement('div');
            ray.className = 'mood-effect-particle sun-ray';
            const angle = (Math.PI * 2 * i) / rayCount;
            const startDist = 25;
            const offsetX = Math.cos(angle) * startDist;
            const offsetY = Math.sin(angle) * startDist;
            
            ray.style.left = (x + offsetX) + 'px';
            ray.style.top = (y + offsetY) + 'px';
            ray.style.setProperty('--ray-angle', angle + 'rad');
            
            container.appendChild(ray);
        }

        this.autoRemove(container, 350);
    },

    // Star effect for excited
    createStarEffect: function (x, y) {
        const container = this.createContainer();
        const particleCount = 20;
        const emoji = '⭐';

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'mood-effect-particle star';
            particle.textContent = emoji;
            
            const angle = Math.random() * Math.PI * 2;
            const distance = 20 + Math.random() * 90;
            const offsetX = Math.cos(angle) * distance;
            const offsetY = Math.sin(angle) * distance;
            
            particle.style.left = (x + offsetX) + 'px';
            particle.style.top = (y + offsetY) + 'px';
            particle.style.animationDelay = (Math.random() * 150) + 'ms';
            
            container.appendChild(particle);
        }

        this.autoRemove(container, 350);
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
