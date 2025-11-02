// Mood emoji visual effects
window.moodEffects = {
    // Create and display mood effect based on selected emoji
    showEffect: function (emoji) {
        // Determine which effect to show
        switch (emoji) {
            case '😭': // Crying - Heavy downpour with puddles
                this.createCryingEffect();
                break;
            case '😢': // Sad - Gentle rain with teardrops
                this.createSadEffect();
                break;
            case '😕': // Confused - Swirling question marks
                this.createConfusedEffect();
                break;
            case '🤔': // Thinking - Light bulbs appearing
                this.createThinkingEffect();
                break;
            case '😐': // Neutral - Calm waves
                this.createNeutralEffect();
                break;
            case '🙂': // Slight smile - Gentle sparkles
                this.createSlightSmileEffect();
                break;
            case '😀': // Happy - Bursting sunshine across screen
                this.createHappyEffect();
                break;
            case '🤩': // Excited - Fireworks and explosions
                this.createExcitedEffect();
                break;
        }
    },

    // 😭 Crying - Heavy downpour with puddles forming
    createCryingEffect: function () {
        const container = this.createContainer();
        const dropCount = 40;
        
        // Heavy rain drops falling
        for (let i = 0; i < dropCount; i++) {
            const drop = document.createElement('div');
            drop.className = 'mood-effect-particle crying-drop';
            drop.textContent = '💧';
            drop.style.left = Math.random() * window.innerWidth + 'px';
            drop.style.top = '-30px';
            drop.style.animationDelay = (Math.random() * 400) + 'ms';
            container.appendChild(drop);
        }
        
        // Puddles forming at bottom
        for (let i = 0; i < 6; i++) {
            const puddle = document.createElement('div');
            puddle.className = 'mood-effect-particle puddle';
            puddle.textContent = '💦';
            puddle.style.left = (Math.random() * 80 + 10) + '%';
            puddle.style.bottom = '0px';
            puddle.style.animationDelay = (500 + Math.random() * 500) + 'ms';
            container.appendChild(puddle);
        }

        this.autoRemove(container, 1600);
    },

    // 😢 Sad - Gentle rain with falling teardrops
    createSadEffect: function () {
        const container = this.createContainer();
        const tearCount = 20;
        
        // Teardrops falling gently
        for (let i = 0; i < tearCount; i++) {
            const tear = document.createElement('div');
            tear.className = 'mood-effect-particle sad-tear';
            tear.textContent = ['💧', '💦'][Math.floor(Math.random() * 2)];
            tear.style.left = Math.random() * window.innerWidth + 'px';
            tear.style.top = '-20px';
            tear.style.animationDelay = (Math.random() * 600) + 'ms';
            container.appendChild(tear);
        }
        
        // Gentle clouds drifting
        for (let i = 0; i < 4; i++) {
            const cloud = document.createElement('div');
            cloud.className = 'mood-effect-particle sad-cloud';
            cloud.textContent = '☁️';
            cloud.style.left = (Math.random() * 100) + '%';
            cloud.style.top = (Math.random() * 40) + '%';
            cloud.style.animationDelay = (i * 200) + 'ms';
            container.appendChild(cloud);
        }

        this.autoRemove(container, 1600);
    },

    // 😕 Confused - Swirling question marks
    createConfusedEffect: function () {
        const container = this.createContainer();
        const questionCount = 15;
        
        // Question marks swirling around
        for (let i = 0; i < questionCount; i++) {
            const question = document.createElement('div');
            question.className = 'mood-effect-particle confused-question';
            question.textContent = ['❓', '❔'][Math.floor(Math.random() * 2)];
            question.style.left = Math.random() * window.innerWidth + 'px';
            question.style.top = Math.random() * window.innerHeight + 'px';
            question.style.animationDelay = (Math.random() * 400) + 'ms';
            container.appendChild(question);
        }
        
        // Dizzy spirals
        for (let i = 0; i < 8; i++) {
            const spiral = document.createElement('div');
            spiral.className = 'mood-effect-particle confused-spiral';
            spiral.textContent = '💫';
            spiral.style.left = Math.random() * window.innerWidth + 'px';
            spiral.style.top = Math.random() * window.innerHeight + 'px';
            spiral.style.animationDelay = (Math.random() * 500) + 'ms';
            container.appendChild(spiral);
        }

        this.autoRemove(container, 1600);
    },

    // 🤔 Thinking - Light bulbs appearing and ideas floating
    createThinkingEffect: function () {
        const container = this.createContainer();
        const ideaCount = 12;
        
        // Light bulbs appearing (idea moments)
        for (let i = 0; i < ideaCount; i++) {
            const bulb = document.createElement('div');
            bulb.className = 'mood-effect-particle thinking-bulb';
            bulb.textContent = '💡';
            bulb.style.left = Math.random() * window.innerWidth + 'px';
            bulb.style.top = Math.random() * window.innerHeight + 'px';
            bulb.style.animationDelay = (Math.random() * 600) + 'ms';
            container.appendChild(bulb);
        }
        
        // Brain and thought bubbles
        for (let i = 0; i < 6; i++) {
            const thought = document.createElement('div');
            thought.className = 'mood-effect-particle thinking-thought';
            thought.textContent = ['🧠', '💭'][Math.floor(Math.random() * 2)];
            thought.style.left = Math.random() * window.innerWidth + 'px';
            thought.style.bottom = '-40px';
            thought.style.animationDelay = (Math.random() * 400) + 'ms';
            container.appendChild(thought);
        }

        this.autoRemove(container, 1600);
    },

    // 😐 Neutral - Calm rippling waves
    createNeutralEffect: function () {
        const container = this.createContainer();
        const waveCount = 10;
        
        // Rippling waves across screen
        for (let i = 0; i < waveCount; i++) {
            const wave = document.createElement('div');
            wave.className = 'mood-effect-particle neutral-wave';
            wave.textContent = '〰️';
            wave.style.left = (i * 10 + Math.random() * 5) + '%';
            wave.style.top = (30 + Math.random() * 40) + '%';
            wave.style.animationDelay = (i * 150) + 'ms';
            container.appendChild(wave);
        }
        
        // Zen circles
        for (let i = 0; i < 5; i++) {
            const circle = document.createElement('div');
            circle.className = 'mood-effect-particle neutral-circle';
            circle.textContent = '⭕';
            circle.style.left = Math.random() * window.innerWidth + 'px';
            circle.style.top = Math.random() * window.innerHeight + 'px';
            circle.style.animationDelay = (Math.random() * 500) + 'ms';
            container.appendChild(circle);
        }

        this.autoRemove(container, 1600);
    },

    // 🙂 Slight smile - Gentle floating hearts and sparkles
    createSlightSmileEffect: function () {
        const container = this.createContainer();
        const particleCount = 20;
        
        // Gentle hearts floating up
        for (let i = 0; i < particleCount; i++) {
            const heart = document.createElement('div');
            heart.className = 'mood-effect-particle smile-heart';
            heart.textContent = ['💗', '💖', '✨'][Math.floor(Math.random() * 3)];
            heart.style.left = Math.random() * window.innerWidth + 'px';
            heart.style.bottom = '-30px';
            heart.style.animationDelay = (Math.random() * 500) + 'ms';
            container.appendChild(heart);
        }
        
        // Soft sparkles
        for (let i = 0; i < 15; i++) {
            const sparkle = document.createElement('div');
            sparkle.className = 'mood-effect-particle smile-sparkle';
            sparkle.textContent = '✨';
            sparkle.style.left = Math.random() * window.innerWidth + 'px';
            sparkle.style.top = Math.random() * window.innerHeight + 'px';
            sparkle.style.animationDelay = (Math.random() * 600) + 'ms';
            container.appendChild(sparkle);
        }

        this.autoRemove(container, 1600);
    },

    // 😀 Happy - Multiple suns bursting across the screen
    createHappyEffect: function () {
        const container = this.createContainer();
        const sunCount = 8;
        
        // Multiple suns appearing across screen
        for (let i = 0; i < sunCount; i++) {
            const sun = document.createElement('div');
            sun.className = 'mood-effect-particle happy-sun';
            sun.textContent = '☀️';
            sun.style.left = Math.random() * window.innerWidth + 'px';
            sun.style.top = Math.random() * window.innerHeight + 'px';
            sun.style.animationDelay = (i * 150) + 'ms';
            container.appendChild(sun);
        }
        
        // Rainbow elements
        for (let i = 0; i < 5; i++) {
            const rainbow = document.createElement('div');
            rainbow.className = 'mood-effect-particle happy-rainbow';
            rainbow.textContent = '🌈';
            rainbow.style.left = (i * 20 + Math.random() * 10) + '%';
            rainbow.style.top = (20 + Math.random() * 30) + '%';
            rainbow.style.animationDelay = (300 + i * 200) + 'ms';
            container.appendChild(rainbow);
        }
        
        // Joy particles
        for (let i = 0; i < 20; i++) {
            const joy = document.createElement('div');
            joy.className = 'mood-effect-particle happy-joy';
            joy.textContent = ['✨', '🌟', '💛'][Math.floor(Math.random() * 3)];
            joy.style.left = Math.random() * window.innerWidth + 'px';
            joy.style.top = Math.random() * window.innerHeight + 'px';
            joy.style.animationDelay = (Math.random() * 700) + 'ms';
            container.appendChild(joy);
        }

        this.autoRemove(container, 1600);
    },

    // 🤩 Excited - Fireworks and celebration explosions
    createExcitedEffect: function () {
        const container = this.createContainer();
        const fireworkCount = 10;
        
        // Fireworks bursting
        for (let i = 0; i < fireworkCount; i++) {
            const firework = document.createElement('div');
            firework.className = 'mood-effect-particle excited-firework';
            firework.textContent = '🎆';
            firework.style.left = (20 + Math.random() * 60) + '%';
            firework.style.top = (20 + Math.random() * 60) + '%';
            firework.style.animationDelay = (Math.random() * 600) + 'ms';
            container.appendChild(firework);
        }
        
        // Exploding stars
        for (let i = 0; i < 25; i++) {
            const star = document.createElement('div');
            star.className = 'mood-effect-particle excited-star';
            star.textContent = ['⭐', '🌟', '✨', '💫'][Math.floor(Math.random() * 4)];
            star.style.left = Math.random() * window.innerWidth + 'px';
            star.style.top = Math.random() * window.innerHeight + 'px';
            star.style.animationDelay = (Math.random() * 500) + 'ms';
            container.appendChild(star);
        }
        
        // Celebration confetti
        for (let i = 0; i < 15; i++) {
            const confetti = document.createElement('div');
            confetti.className = 'mood-effect-particle excited-confetti';
            confetti.textContent = '🎉';
            confetti.style.left = Math.random() * window.innerWidth + 'px';
            confetti.style.top = '-20px';
            confetti.style.animationDelay = (Math.random() * 400) + 'ms';
            container.appendChild(confetti);
        }

        this.autoRemove(container, 1600);
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
