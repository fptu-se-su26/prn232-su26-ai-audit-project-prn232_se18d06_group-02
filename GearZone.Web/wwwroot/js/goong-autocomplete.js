class GoongAutocomplete {
    constructor(apiKey, options = {}) {
        this.apiKey = apiKey;
        this.inputElement = options.inputElement;
        this.resultsElement = options.resultsElement;
        this.onSelect = options.onSelect;
        this.debounceTimeout = null;
        this.init();
    }

    init() {
        if (!this.inputElement) return;

        this.inputElement.addEventListener('input', () => {
            clearTimeout(this.debounceTimeout);
            const query = this.inputElement.value.trim();
            
            if (query.length < 3) {
                this.hideResults();
                return;
            }

            this.debounceTimeout = setTimeout(() => this.search(query), 300);
        });

        // Hide results when clicking outside
        document.addEventListener('click', (e) => {
            if (e.target !== this.inputElement && e.target !== this.resultsElement) {
                this.hideResults();
            }
        });
    }

    async search(query) {
        try {
            const response = await fetch(`https://rsapi.goong.io/Place/AutoComplete?api_key=${this.apiKey}&input=${encodeURIComponent(query)}`);
            const data = await response.json();

            if (data.status === 'OK') {
                this.showResults(data.predictions);
            } else {
                this.hideResults();
            }
        } catch (error) {
            console.error('Goong Autocomplete Error:', error);
            this.hideResults();
        }
    }

    showResults(predictions) {
        if (!this.resultsElement) return;

        this.resultsElement.innerHTML = '';
        this.resultsElement.classList.remove('hidden');

        predictions.forEach(prediction => {
            const item = document.createElement('div');
            item.className = 'px-4 py-2 hover:bg-slate-100 dark:hover:bg-slate-700 cursor-pointer text-sm text-slate-700 dark:text-slate-300 border-b border-slate-100 dark:border-slate-800 last:border-0';
            item.innerHTML = `
                <div class="font-bold">${prediction.structured_formatting.main_text}</div>
                <div class="text-xs text-slate-500">${prediction.structured_formatting.secondary_text}</div>
            `;
            item.addEventListener('click', () => this.select(prediction));
            this.resultsElement.appendChild(item);
        });
    }

    hideResults() {
        if (this.resultsElement) {
            this.resultsElement.classList.add('hidden');
        }
    }

    async select(prediction) {
        this.inputElement.value = prediction.description;
        this.hideResults();

        if (this.onSelect) {
            try {
                const response = await fetch(`https://rsapi.goong.io/Place/Detail?api_key=${this.apiKey}&place_id=${prediction.place_id}`);
                const data = await response.json();

                if (data.status === 'OK') {
                    // Pass the original prediction description along with the place details
                    data.result.original_description = prediction.description;
                    this.onSelect(data.result);
                }
            } catch (error) {
                console.error('Goong Place Detail Error:', error);
            }
        }
    }
}
