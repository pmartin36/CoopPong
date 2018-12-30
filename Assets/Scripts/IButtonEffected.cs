public interface IButtonEffected {
	float Amount { get; set; }
	void AddActor(ButtonLocation location, float amount);
}